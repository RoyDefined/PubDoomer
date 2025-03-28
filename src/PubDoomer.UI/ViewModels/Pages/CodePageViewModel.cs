﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PubDoomer.Engine.Orchestration;
using PubDoomer.Engine.Static;
using PubDoomer.Engine.Tasks;
using PubDoomer.Project;
using PubDoomer.Project.Profile;
using PubDoomer.Project.Run;
using PubDoomer.Project.Tasks;
using PubDoomer.Saving;
using PubDoomer.Services;
using PubDoomer.Utils.MergedSettings;
using PubDoomer.ViewModels.Dialogues;

namespace PubDoomer.ViewModels.Pages;

// TODO: Proper syntax highlighting for the code editor. Currently c# but even better would be proper ACS support.
public partial class CodePageViewModel : PageViewModel
{
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
        
        // Some editor code to get started.
        EditorDocument.Text = """
            strict namespace
            {
                script "PBOpen" open
                {
                    LogMessage("Hello, PubDoomer!");
                }
            
                private void LogMessage(str message)
                {
                    Print(s:message);
                }
            }
            """;
        
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
        
        // Some editor code to get started.
        EditorDocument.Text = """
            // Code goes here.
            """;
        
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
        var engineTask = compileTask.ToEngineTaskBase();
        var runTask = new ProfileRunTask(
            ProfileTaskErrorBehaviour.StopOnError,
            engineTask);

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
        
        var context = SettingsMerger.Merge(CurrentProjectProvider.ProjectContext, _settings);

        // TODO: Merge with the orchestrator.
        foreach (var runTask in runTasks)
        {
            runTask.Status = ProfileRunTaskStatus.Running;

            var result = await _projectTaskOrchestrator.InvokeTaskAsync(runTask.Task, context);
            _logger.LogDebug("Task result: {ResultType}, {Result}", result.ResultType, result.ResultMessage);

            runTask.Status = result.ResultType == TaskResultType.Success
                ? ProfileRunTaskStatus.Success
                : ProfileRunTaskStatus.Error;

            runTask.ResultMessage = result.ResultMessage;
            runTask.ResultWarnings = result.Warnings != null ? new ObservableCollection<string>(result.Warnings) : null;
            runTask.ResultErrors = result.Errors != null ? new ObservableCollection<string>(result.Errors) : null;
            runTask.Exception = result.Exception;
            
            // Check error behaviour.
            // If there was an error and the behaviour is to quit, then end the task invokation early.
            if (runTask.Status == ProfileRunTaskStatus.Error)
            {
                if (runTask.Behaviour == ProfileTaskErrorBehaviour.StopOnError)
                {
                    _logger.LogWarning(runTask.Exception, "Task failure.");
                    break;
                }
                else
                {
                    _logger.LogWarning(runTask.Exception, "Task failed but is configured to not stop on errors. Execution will continue.");
                }
            }
        }
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
        AvailableCompilerTasks.Add(new AccCompileTask() { Name = taskName, InputFilePath = _temporaryFileInputPath });
        AvailableCompilerTasks.Add(new BccCompileTask() { Name = taskName, InputFilePath = _temporaryFileInputPath });
        AvailableCompilerTasks.Add(new GdccAccCompileTask() { Name = taskName, InputFilePath = _temporaryFileInputPath });
    }

    [MemberNotNullWhen(false, nameof(_dialogueProvider), nameof(_settings))]
    private bool AssertInDesignMode()
    {
        return Design.IsDesignMode;
    }
}