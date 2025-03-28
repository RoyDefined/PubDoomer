using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Utilities;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PubDoomer.Engine.Abstract;
using PubDoomer.Engine.TaskInvokation.Orchestration;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using PubDoomer.Project;
using PubDoomer.Project.Run;
using PubDoomer.Project.Tasks;
using PubDoomer.Saving;
using PubDoomer.Services;
using PubDoomer.Settings.Merged;
using PubDoomer.Tasks.AcsVM;
using PubDoomer.Tasks.Compile;
using PubDoomer.Tasks.Compile.Acc;
using PubDoomer.Tasks.Compile.Bcc;
using PubDoomer.Tasks.Compile.GdccAcc;
using PubDoomer.Utils.TaskInvokation;
using PubDoomer.ViewModels.Dialogues;

namespace PubDoomer.ViewModels.Pages;

// TODO: Proper syntax highlighting for the code editor. Currently c# but even better would be proper ACS support.
public partial class CodePageViewModel : PageViewModel
{
    /* Code templates to display for design time and the different compilers */
    private const string DesignTimeCode = BccCode;

    private const string AccCode = """
        #include "zcommon.acs"
    
        script 1 open
        {
            // Code goes here.
            LogMessage("Hello, PubDoomer!");
        }
    
        function void LogMessage(str message)
        {
            Print(s:message);
        }
        """;

    private const string BccCode = """
        #import "zcommon.bcs"
        
        strict namespace
        {
            script "PBOpen" open
            {
                // Code goes here.
                Utils::LogMessage("Hello, PubDoomer!");
            }
        }
        
        strict namespace Utils
        {
            private void LogMessage(str message)
            {
                Print(s:message);
            }
        }
        """;
    
    
    private const string GdccAccCode = """
        #include "zcommon.acs"
        
        // Function comes first, or GDCC-ACC will warn us of forward references.
        function void LogMessage(str message)
        {
            Print("Hello, %s:!", message);
        }
    
        script "PBOpen" open
        {
            LogMessage("PubDoomer");
        }
        """;

    /// <summary>
    /// Represents mappings of a compiler type to a code template.
    /// </summary>
    private static readonly Dictionary<CompilerType, string> CompilerToTemplateMap = new()
    {
        [CompilerType.Acc] = AccCode,
        [CompilerType.Bcc] = BccCode,
        [CompilerType.GdccAcc] = GdccAccCode,
    };
    
    // Paths used for the editor code.
    private readonly string _temporaryFileInputPath = Path.Combine(EngineStatics.TemporaryDirectory, "Editor", "code-editor-file.temp");
    private readonly string _temporaryFileOutputPath = Path.Combine(EngineStatics.TemporaryDirectory, "Editor", "output.o");
    
    // Dependencies
    private readonly ILogger _logger;
    private readonly ProjectTaskOrchestrator _projectTaskOrchestrator;
    private readonly DialogueProvider? _dialogueProvider;
    private readonly LocalSettings? _settings;
    
    // The task base that handles the ACS VM process.
    private AcsVirtualMachineExecuteTask? _acsVmTask;
    
    /// <summary>
    /// The available compilation types.
    /// </summary>
    [ObservableProperty] private ObservableCollection<CompileTaskBase> _availableCompilerTasks = [];
    
    /// <summary>
    /// The current task to be used for compilation.
    /// </summary>
    [ObservableProperty] private CompileTaskBase? _selectedCompilationTask;
    
    /// <summary>
    /// The code that is written in the editor.
    /// </summary>
    [ObservableProperty] private TextDocument _editorDocument = new TextDocument();
    
    /// <summary>
    /// Represents the tasks that have been invoked.
    /// <br />These will be set when a certain action is started, in which case this collection has its tasks supplied.
    /// <br />The output container will display these tasks in order.
    /// </summary>
    [ObservableProperty] private ObservableCollection<ProfileRunTask> _invokedTasks = [];

    /// <summary>
    /// If <c>true</c> the code editor was changed since its initial setup.
    /// <br /> This is used to determine if the code editor should have its template replaced without modifying user code.
    /// </summary>
    private bool _codeEditorIsDirty;

    /// <summary>
    /// Design mode view model constructor
    /// </summary>
    public CodePageViewModel()
    {
        if (!Design.IsDesignMode) throw new InvalidOperationException();
        
        // Dependencies
        _logger = new NullLogger<CodePageViewModel>();
        CurrentProjectProvider = new CurrentProjectProvider();
        
        // TODO: Duplicate code in the profiles page.
        _projectTaskOrchestrator = new ProjectTaskOrchestrator(
            NullLogger<ProjectTaskOrchestrator>.Instance,
            ((type, task, context) => Activator.CreateInstance(type, task, context) as ITaskHandler));
        
        EditorDocument.Text = DesignTimeCode;
        WeakSubscribeToDocumentChanges(EditorDocument);
        PopulateAvailableCompilerTasks();
    }

    /// <summary>
    /// Non-design constructor for this view model.
    /// </summary>
    public CodePageViewModel(
        ILogger<CodePageViewModel> logger,
        CurrentProjectProvider currentProjectProvider,
        ProjectTaskOrchestrator projectTaskOrchestrator,
        DialogueProvider dialogueProvider,
        LocalSettings settings)
    {
        // Dependencies
        _logger = logger;
        CurrentProjectProvider = currentProjectProvider;
        _projectTaskOrchestrator = projectTaskOrchestrator;
        _dialogueProvider = dialogueProvider;
        _settings = settings;
        
        EditorDocument.Text = AccCode;
        WeakSubscribeToDocumentChanges(EditorDocument);
        PopulateAvailableCompilerTasks();
    }
    
    /// <remarks>The project provider can return no project in this context as it is optional.</remarks>
    public CurrentProjectProvider CurrentProjectProvider { get; }

    /// <summary>
    /// Compiles the editor context. If succeeded, opens the file output directory.
    /// </summary>
    [RelayCommand]
    private async Task CompileAsync()
    {
        if (AssertInDesignMode()) return;
        
        if (SelectedCompilationTask == null)
        {
            await _dialogueProvider.AlertAsync(AlertType.Warning, "No compiler selected.", "Please select a compiler to compile with.");
            return;
        }

        await RunTasksAsync(
            await PrepareCompilationTaskAsync());
    }
    
    /// <summary>
    /// Compiles the editor context and runs it using ACS VM, assuming compilation succeeded.
    /// </summary>
    [RelayCommand]
    private async Task CompileAndRunAsync()
    {
        if (AssertInDesignMode()) return;
        
        if (SelectedCompilationTask == null)
        {
            await _dialogueProvider.AlertAsync(AlertType.Warning, "No compiler selected.", "Please select a compiler to compile and run with.");
            return;
        }

        var compileTask = await PrepareCompilationTaskAsync();
        var vmTask = PrepareAcsVmTask();
        await RunTasksAsync([ compileTask, vmTask ]);
    }

    /// <summary>
    /// Prepares a task to compile.
    /// </summary>
    private async Task<ProfileRunTask> PrepareCompilationTaskAsync()
    {
        Debug.Assert(SelectedCompilationTask != null);
        
        // Write file to temporary output, because the compiler need a file to work with.
        var code = EditorDocument.Text;
        Directory.CreateDirectory(Path.GetDirectoryName(_temporaryFileOutputPath)!);
        await File.WriteAllTextAsync(_temporaryFileInputPath, code);
        
        // Clone current task.
        // We also append an output filepath so we can access the resulting file.
        var compileTask = SelectedCompilationTask.DeepClone();
        compileTask.OutputFilePath = _temporaryFileOutputPath;
        
        // Set up the task
        var runTask = new ProfileRunTask(
            ProfileTaskErrorBehaviour.StopOnError,
            compileTask);

        return runTask;
    }
    
    private ProfileRunTask PrepareAcsVmTask()
    {
        Debug.Assert(_acsVmTask != null);
        
        var runTask = new ProfileRunTask(
            ProfileTaskErrorBehaviour.StopOnError,
            _acsVmTask);

        return runTask;
    }
    
    /// <summary>
    /// Main method that runs all the created tasks.
    /// </summary>
    private async Task RunTasksAsync(params IList<ProfileRunTask> runTasks)
    {
        // Update the invoked tasks.
        InvokedTasks.Clear();
        foreach (var runTask in runTasks)
        {
            InvokedTasks.Add(runTask);
        }

        // Set up profile and context
        var profile = new ProfileRunContext("Code editor run profile", runTasks);
        var settings = SettingsMerger.Merge(CurrentProjectProvider.ProjectContext, _settings);
        var context = TaskInvokeContextUtil.BuildContext(settings);

        // TODO: Make use of the profile.
        await _projectTaskOrchestrator.InvokeProfileAsync(profile, context);
    }
    
    private void WeakSubscribeToDocumentChanges(TextDocument editorDocument)
    {
        WeakEventHandlerManager.Subscribe<TextDocument, EventArgs, CodePageViewModel>(
            editorDocument, nameof(TextDocument.TextChanged), OnTextDocumentTextChanged);
    }

    private void OnTextDocumentTextChanged(object? _, EventArgs __)
    {
        // Unconditionally set as dirty.
        _codeEditorIsDirty = true;
    }

    private void PopulateAvailableCompilerTasks()
    {
        Debug.Assert(AvailableCompilerTasks.Count == 0);

        _acsVmTask = new AcsVirtualMachineExecuteTask()
        {
            Name = "Code output",
            InputFilePath = _temporaryFileOutputPath,
        };

        const string taskName = "Compiler";
        AvailableCompilerTasks.Add(new ObservableAccCompileTask() { Name = taskName, InputFilePath = _temporaryFileInputPath });
        AvailableCompilerTasks.Add(new ObservableBccCompileTask() { Name = taskName, InputFilePath = _temporaryFileInputPath });
        AvailableCompilerTasks.Add(new ObservableGdccAccCompileTask() { Name = taskName, InputFilePath = _temporaryFileInputPath });
    }

    partial void OnSelectedCompilationTaskChanged(CompileTaskBase? value)
    {
        if (value == null) return;
        
        // Replace the code with a new template unless dirty.
        if (!_codeEditorIsDirty)
        {
            EditorDocument.Text = CompilerToTemplateMap[value.Type];
            _codeEditorIsDirty = false;
        }
    }

    [MemberNotNullWhen(false, nameof(_dialogueProvider), nameof(_settings))]
    private bool AssertInDesignMode()
    {
        return Design.IsDesignMode;
    }
}