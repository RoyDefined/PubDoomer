using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PubDoomer.Objects;
using PubDoomer.Project.Tasks;
using PubDoomer.Services;
using PubDoomer.Tasks.Compile.Acc;
using PubDoomer.Tasks.Compile.Bcc;
using PubDoomer.Tasks.Compile.GdccAcc;
using PubDoomer.Tasks.Compile.GdccCc;
using PubDoomer.Tasks.Compile.Observables;
using PubDoomer.Tasks.FileSystem;
using PubDoomer.Tasks.FileSystem.CopyProject;

namespace PubDoomer.ViewModels.Dialogues;

public partial class CreateOrEditTaskWindowViewModel : ViewModelBase
{
    private static readonly Dictionary<string, string> _typeToFilePickerMessage = new()
    {
        ["accinput"] = "Pick ACC compatible ACS file",
        ["accoutput"] = "Pick compiled ACC output",
        ["bccinput"] = "Pick BCC compatible ACS file",
        ["bccoutput"] = "Pick compiled ACC output",
        ["gdccaccinput"] = "Pick GDCC-ACC compatible ACS file",
        ["gdccaccoutput"] = "Pick compiled GDCC-ACC output",
    };
    
    private readonly WindowProvider? _windowProvider;

    // Form data
    // The available task types.
    [ObservableProperty] private ObservableCollection<ProjectTaskBase> _availableTaskTypes =
    [
        new ObservableAccCompileTask(),
        new ObservableBccCompileTask(),
        new ObservableGdccAccCompileTask(),
        new ObservableGdccCcCompileTask(),
        new ObservableCopyProjectTask()
    ];

    [ObservableProperty] private string _createOrEditButtonText;

    // The currently selected task.
    [ObservableProperty] private ProjectTaskBase? _currentTask;

    // Form visuals
    [ObservableProperty] private string _windowTitle;
    
    public CreateOrEditTaskWindowViewModel()
    {
        if (!Design.IsDesignMode) throw new InvalidOperationException();

        WindowTitle = "Add new task (designer)";
        CreateOrEditButtonText = "Add";
    }

    public CreateOrEditTaskWindowViewModel(
        WindowProvider windowProvider)
    {
        _windowProvider = windowProvider;

        WindowTitle = "Add new task";
        CreateOrEditButtonText = "Add";

        SubscribeTaskChanges();
    }

    public CreateOrEditTaskWindowViewModel(
        WindowProvider windowProvider,
        ProjectTaskBase task)
    {
        _windowProvider = windowProvider;

        // Replace the task in the available task list.
        // This way the drop down also respects the task correctly and we don't accidentally replace it with an empty one.
        var taskType = task.GetType();
        var taskToReplace = AvailableTaskTypes.Single(t => t.GetType() == taskType);
        var index = AvailableTaskTypes.IndexOf(taskToReplace);
        AvailableTaskTypes[index] = task;

        CurrentTask = task;

        WindowTitle = "Edit task";
        CreateOrEditButtonText = "Edit";

        SubscribeTaskChanges();
    }

    public bool FormIsValid => CurrentTask switch
    {
        // TODO: IsNullOrWhitespace, not IsNullOrEmpty.
        // TODO: This should be part of the task.
        ObservableAccCompileTask compileTask => !string.IsNullOrEmpty(compileTask.InputFilePath) && !string.IsNullOrEmpty(compileTask.OutputFilePath),
        ObservableBccCompileTask compileTask => !string.IsNullOrEmpty(compileTask.InputFilePath) && !string.IsNullOrEmpty(compileTask.OutputFilePath),
        ObservableGdccAccCompileTask compileTask => !string.IsNullOrEmpty(compileTask.InputFilePath) && !string.IsNullOrEmpty(compileTask.OutputFilePath),
        ObservableGdccCcCompileTask compileTask => !string.IsNullOrEmpty(compileTask.InputFilePath) && !string.IsNullOrEmpty(compileTask.OutputFilePath),
        ObservableCopyProjectTask copyProjectTask => !string.IsNullOrWhiteSpace(copyProjectTask.TargetFolder) || copyProjectTask.UseTempFolder,
        _ => false
    };

    // This hook is created for all the available task types so `FormIsValid` can be hooked to INPC calls and update the form.
    private void SubscribeTaskChanges()
    {
        foreach (var inpcObject in AvailableTaskTypes.Cast<INotifyPropertyChanged>())
            inpcObject.PropertyChanged += UpdateFormValid;
    }

    private void UpdateFormValid(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(FormIsValid));
    }

    /// <summary>
    /// Command specific to tasks which adds a field which specifies a directory to search in for files.
    /// </summary>
    [RelayCommand]
    private void AddIncludeDirectory()
    {
        if (CurrentTask is not ObservableBccCompileTask task)
        {
            return;
        }
        
        task.IncludeDirectories.Add(new());
    }
    
    [RelayCommand]
    private void RemoveIncludeDirectory(ObservableString value)
    {
        if (CurrentTask is not ObservableBccCompileTask task)
        {
            return;
        }
        
        task.IncludeDirectories.Remove(value);
    }

    [RelayCommand]
    private async Task PickFileAsync(TaskCreateDialogueFilePickerType taskType)
    {
        if (AssertInDesignMode()) return;

        var window = _windowProvider.ProvideWindow();
        var filePicker = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = _typeToFilePickerMessage[taskType.CompilerType + taskType.FileType],
            AllowMultiple = false
        });

        if (filePicker.Count == 0) return;

        var filePath = filePicker.First().Path.LocalPath;
        if (CurrentTask is not CompileTaskBase compileTask)
        {
            Debug.Fail("The task should be a compile task.");
            return;
        }

        if (taskType.FileType == "input")
        {
            compileTask.InputFilePath = filePath;
        }
        else
        {
            compileTask.OutputFilePath = filePath;
        }
    }
    
    [MemberNotNullWhen(false, nameof(_windowProvider))]
    private bool AssertInDesignMode()
    {
        return Design.IsDesignMode;
    }
}