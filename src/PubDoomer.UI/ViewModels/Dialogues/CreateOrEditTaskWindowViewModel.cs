using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PubDoomer.Objects;
using PubDoomer.Project;
using PubDoomer.Project.Tasks;
using PubDoomer.Services;
using PubDoomer.Tasks.Compile.Acc;
using PubDoomer.Tasks.Compile.Bcc;
using PubDoomer.Tasks.Compile.GdccAcc;
using PubDoomer.Tasks.Compile.GdccCc;
using PubDoomer.Tasks.Compile.Observables;
using PubDoomer.Tasks.FileSystem;
using PubDoomer.Tasks.FileSystem.CopyFile;
using PubDoomer.Tasks.FileSystem.CopyFolder;
using PubDoomer.Tasks.FileSystem.CopyProject;
using PubDoomer.Tasks.FileSystem.DeleteFile;
using PubDoomer.Tasks.FileSystem.DeleteFolder;
using PubDoomer.Tasks.FileSystem.MoveFile;
using PubDoomer.Tasks.FileSystem.MoveFolder;
using PubDoomer.Tasks.FileSystem.ZipFolder;

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
    private readonly CurrentProjectProvider _currentProjectProvider;

    // Form data
    // The available task types.
    [ObservableProperty] private ObservableCollection<ProjectTaskBase> _availableTaskTypes =
    [
        new ObservableAccCompileTask(),
        new ObservableBccCompileTask(),
        new ObservableGdccAccCompileTask(),
        new ObservableGdccCcCompileTask(),
        new ObservableCopyProjectTask(),
        new ObservableCopyFolderTask(),
        new ObservableMoveFolderTask(),
        new ObservableCopyFileTask(),
        new ObservableMoveFileTask(),
        new ObservableZipFolderTask(),
        new ObservableDeleteFolderTask(),
        new ObservableDeleteFileTask(),
    ];

    [ObservableProperty] private string _createOrEditButtonText;

    // The currently selected task.
    [ObservableProperty] private ProjectTaskBase? _currentTask;

    // Form visuals
    [ObservableProperty] private string _windowTitle;
    
    public CreateOrEditTaskWindowViewModel()
    {
        if (!Design.IsDesignMode) throw new InvalidOperationException();
        
        _currentProjectProvider = new();
        
        WindowTitle = "Add new task (designer)";
        CreateOrEditButtonText = "Add";
    }

    public CreateOrEditTaskWindowViewModel(
        WindowProvider windowProvider,
        CurrentProjectProvider currentProjectProvider)
    {
        _windowProvider = windowProvider;
        _currentProjectProvider = currentProjectProvider;

        WindowTitle = "Add new task";
        CreateOrEditButtonText = "Add";

        SubscribeTaskChanges();
    }

    public CreateOrEditTaskWindowViewModel(
        WindowProvider windowProvider,
        CurrentProjectProvider currentProjectProvider,
        ProjectTaskBase task)
    {
        _windowProvider = windowProvider;
        _currentProjectProvider = currentProjectProvider;

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
        ObservableCopyFolderTask copyFolderTask => !string.IsNullOrWhiteSpace(copyFolderTask.SourceFolder) || !string.IsNullOrWhiteSpace(copyFolderTask.TargetFolder) || copyFolderTask.Recursive,
        ObservableMoveFolderTask moveFolderTask => !string.IsNullOrWhiteSpace(moveFolderTask.SourceFolder) || !string.IsNullOrWhiteSpace(moveFolderTask.TargetFolder) || moveFolderTask.Recursive,
        ObservableCopyFileTask copyFileTask => !string.IsNullOrWhiteSpace(copyFileTask.SourceFile) || !string.IsNullOrWhiteSpace(copyFileTask.TargetFile),
        ObservableMoveFileTask moveFileTask => !string.IsNullOrWhiteSpace(moveFileTask.SourceFile) || !string.IsNullOrWhiteSpace(moveFileTask.TargetFile),
        ObservableZipFolderTask zipFolderTask => !string.IsNullOrWhiteSpace(zipFolderTask.SourceFolder) || !string.IsNullOrWhiteSpace(zipFolderTask.TargetFilePath),
        ObservableDeleteFolderTask deleteFolderTask => !string.IsNullOrWhiteSpace(deleteFolderTask.TargetFolder),
        ObservableDeleteFileTask deleteFileTask => !string.IsNullOrWhiteSpace(deleteFileTask.TargetFilePath),
        _ => false
    };
    
    // Called when a valid task is about to be saved.
    // This method handles task specific behaviour that must be handled when saving occurs.
    public void OnSaveTask()
    {
        // TODO: This should be part of the task. This method should call the method on the task.
        var basePath = _currentProjectProvider.ProjectContext?.FolderPath;
        Debug.Assert(!string.IsNullOrWhiteSpace(basePath), "Project context base folder must be set.");

        string MakeRelative(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            if (!Path.IsPathRooted(input)) return input;
            var fullPath = Path.GetFullPath(input);
            var relative = Path.GetRelativePath(basePath, fullPath);
            return relative;
        }

        switch (CurrentTask)
        {
            case ObservableAccCompileTask acc:
                acc.InputFilePath = MakeRelative(acc.InputFilePath);
                acc.OutputFilePath = MakeRelative(acc.OutputFilePath);

                foreach (var include in acc.IncludeDirectories)
                {
                    include.Value = MakeRelative(include.Value);
                }
                
                acc.DebugFilePath = MakeRelative(acc.DebugFilePath);
                break;

            case ObservableBccCompileTask bcc:
                bcc.InputFilePath = MakeRelative(bcc.InputFilePath);
                bcc.OutputFilePath = MakeRelative(bcc.OutputFilePath);

                foreach (var include in bcc.IncludeDirectories)
                {
                    include.Value = MakeRelative(include.Value);
                }
                break;

            case ObservableGdccAccCompileTask gdccAcc:
                gdccAcc.InputFilePath = MakeRelative(gdccAcc.InputFilePath);
                gdccAcc.OutputFilePath = MakeRelative(gdccAcc.OutputFilePath);

                foreach (var include in gdccAcc.IncludeDirectories)
                {
                    include.Value = MakeRelative(include.Value);
                }
                break;

            case ObservableGdccCcCompileTask gdccCc:
                gdccCc.InputFilePath = MakeRelative(gdccCc.InputFilePath);
                gdccCc.OutputFilePath = MakeRelative(gdccCc.OutputFilePath);
                break;

            case ObservableCopyProjectTask copyProj:
                if (!string.IsNullOrWhiteSpace(copyProj.TargetFolder)) copyProj.TargetFolder = MakeRelative(copyProj.TargetFolder);
                break;

            case ObservableCopyFolderTask copyFolder:
                if (!string.IsNullOrWhiteSpace(copyFolder.SourceFolder)) copyFolder.SourceFolder = MakeRelative(copyFolder.SourceFolder);
                if (!string.IsNullOrWhiteSpace(copyFolder.TargetFolder)) copyFolder.TargetFolder = MakeRelative(copyFolder.TargetFolder);
                break;

            case ObservableMoveFolderTask moveFolder:
                if (!string.IsNullOrWhiteSpace(moveFolder.SourceFolder)) moveFolder.SourceFolder = MakeRelative(moveFolder.SourceFolder);
                if (!string.IsNullOrWhiteSpace(moveFolder.TargetFolder)) moveFolder.TargetFolder = MakeRelative(moveFolder.TargetFolder);
                break;

            case ObservableCopyFileTask copyFile:
                if (!string.IsNullOrWhiteSpace(copyFile.SourceFile)) copyFile.SourceFile = MakeRelative(copyFile.SourceFile);
                if (!string.IsNullOrWhiteSpace(copyFile.TargetFile)) copyFile.TargetFile = MakeRelative(copyFile.TargetFile);
                break;

            case ObservableMoveFileTask moveFile:
                if (!string.IsNullOrWhiteSpace(moveFile.SourceFile)) moveFile.SourceFile = MakeRelative(moveFile.SourceFile);
                if (!string.IsNullOrWhiteSpace(moveFile.TargetFile)) moveFile.TargetFile = MakeRelative(moveFile.TargetFile);
                break;

            case ObservableZipFolderTask zipFolder:
                if (!string.IsNullOrWhiteSpace(zipFolder.SourceFolder)) zipFolder.SourceFolder = MakeRelative(zipFolder.SourceFolder);
                if (!string.IsNullOrWhiteSpace(zipFolder.TargetFilePath)) zipFolder.TargetFilePath = MakeRelative(zipFolder.TargetFilePath);
                break;

            case ObservableDeleteFolderTask deleteFolder:
                if (!string.IsNullOrWhiteSpace(deleteFolder.TargetFolder)) deleteFolder.TargetFolder = MakeRelative(deleteFolder.TargetFolder);
                break;

            case ObservableDeleteFileTask deleteFile:
                if (!string.IsNullOrWhiteSpace(deleteFile.TargetFilePath)) deleteFile.TargetFilePath = MakeRelative(deleteFile.TargetFilePath);
                break;
        }
    }

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
        if (CurrentTask is ObservableAccCompileTask accTask)
            accTask.IncludeDirectories.Add(new());
        
        if (CurrentTask is ObservableBccCompileTask bccTask)
            bccTask.IncludeDirectories.Add(new());
        
        if (CurrentTask is ObservableGdccAccCompileTask gdccAccTask)
            gdccAccTask.IncludeDirectories.Add(new());
    }
    
    [RelayCommand]
    private void RemoveIncludeDirectory(ObservableString value)
    {
        if (CurrentTask is ObservableAccCompileTask accTask)
            accTask.IncludeDirectories.Remove(value);
        
        if (CurrentTask is ObservableBccCompileTask bccTask)
            bccTask.IncludeDirectories.Remove(value);
        
        if (CurrentTask is ObservableGdccAccCompileTask gdccAccTask)
            gdccAccTask.IncludeDirectories.Remove(value);
    }

    [RelayCommand]
    private void AddMacro()
    {
        if (CurrentTask is ObservableBccCompileTask bccTask)
            bccTask.Macros.Add(new());
        
        if (CurrentTask is ObservableGdccAccCompileTask gdccAccTask)
            gdccAccTask.Macros.Add(new());
    }
    
    [RelayCommand]
    private void RemoveMacro(ObservableString value)
    {
        if (CurrentTask is ObservableBccCompileTask bccTask)
            bccTask.Macros.Remove(value);
        
        if (CurrentTask is ObservableGdccAccCompileTask gdccAccTask)
            gdccAccTask.Macros.Remove(value);
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