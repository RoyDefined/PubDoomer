using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PubDoomer.Context;
using PubDoomer.Engine.Saving;
using PubDoomer.Factory;
using PubDoomer.Logging;
using PubDoomer.Project;
using PubDoomer.Saving;
using PubDoomer.Services;
using PubDoomer.Settings.Local;
using PubDoomer.Settings.Project;
using PubDoomer.Settings.Recent;
using PubDoomer.ViewModels.Dialogues;

namespace PubDoomer.ViewModels;

// This model is specific for the desktop window so it violates the pattern a bit.
public partial class MainWindowModel : MainViewModel
{
    private readonly DialogueProvider? _dialogueProvider;
    private readonly ProjectSavingService? _savingService;
    private readonly LocalSettingsService? _localSettingsService;
    private readonly RecentProjectsService? _recentProjectsService;
    private readonly WindowProvider? _windowProvider;

    [ObservableProperty] private RecentProjectCollection _recentProjects;
    [ObservableProperty] private int _recentProjectIndex;

    public MainWindowModel()
    {
        if (!Design.IsDesignMode) throw new InvalidOperationException();

        _recentProjects =
        [
            new RecentProject("Project Foo", "Path/To/Foo"),
            new RecentProject("Project Bar", "Path/To/Bar")
        ];
    }

    public MainWindowModel(
        ILogger<MainWindowModel> logger,
        LogEmitter logEmitter,
        PubDoomerEnvironment environment,
        SessionSettings sessionSettings,
        CurrentProjectProvider currentProjectProvider,
        PageViewModelFactory pageViewModelFactory,
        WindowNotificationManager windowNotificationManager,
        RecentProjectCollection recentProjects,
        ProjectSavingService savingService,
        LocalSettingsService localSettingsService,
        RecentProjectsService recentProjectsService,
        WindowProvider windowProvider,
        DialogueProvider dialogueProvider)
        : base(logger, logEmitter, environment, sessionSettings, currentProjectProvider, pageViewModelFactory, windowNotificationManager)
    {
        _savingService = savingService;
        _localSettingsService = localSettingsService;
        _recentProjectsService = recentProjectsService;

        _windowProvider = windowProvider;
        _dialogueProvider = dialogueProvider;
        _recentProjects = recentProjects;

        // Load local data in the background.
        _ = Task.Run(LoadLocalDataAsync);
    }

    public bool WindowEnlarged => _windowProvider?.TryProvideWindow(out var window) == true
                                  && window.WindowState == WindowState.Maximized;

    // Window chrome commands
    [RelayCommand]
    private void MinimizeWindow()
    {
        if (AssertInDesignMode()) return;

        _windowProvider.ProvideWindow().WindowState = WindowState.Minimized;
    }

    [RelayCommand]
    private void ToggleEnlargeWindow()
    {
        if (AssertInDesignMode()) return;

        var window = _windowProvider.ProvideWindow();
        window.WindowState = window.WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(WindowEnlarged)));
    }

    [RelayCommand]
    private void CloseWindow()
    {
        if (AssertInDesignMode()) return;
        _windowProvider.ProvideWindow().Close();
    }

    // Menu commands

    [RelayCommand]
    private async Task NewProjectAsync()
    {
        if (AssertInDesignMode()) return;

        var vm = new CreateOrEditProjectWindowViewModel(_windowProvider);
        var result = await _dialogueProvider.GetCreateOrEditDialogueWindowAsync(vm);
        if (!result) return;
        
        CurrentProjectProvider.ProjectContext = vm.Project;
        if (!await SaveProjectCoreAsync()) return;
        
        WindowNotificationManager?.Show(new Notification("Project created", "The project has been created successfully.",
            NotificationType.Success));
    }

    [RelayCommand]
    private async Task SaveProjectAsync()
    {
        if (AssertInDesignMode()) return;
        
        try
        {
            if (!await SaveProjectCoreAsync()) return;
            WindowNotificationManager?.Show(new Notification("Project saved", null, NotificationType.Success));
        }
        catch (Exception ex)
        {
            await _dialogueProvider.AlertAsync(AlertType.Warning,
                ex,
                "Failed to save project",
                "The project could not be saved.");
        }
    }

    private async Task<bool> SaveProjectCoreAsync()
    {
        if (AssertInDesignMode()) return false;

        var projectContext = CurrentProjectProvider.ProjectContext;
        if (projectContext == null)
        {
            return false;
        }

        await using var fileStream = File.OpenWrite(projectContext.GetFullPath());
        _savingService.SaveProject(projectContext, fileStream);
        return true;
    }

    [RelayCommand]
    private async Task SaveProjectAsAsync()
    {
        if (AssertInDesignMode()) return;
        
        try
        {
            if (!await SaveProjectAsCoreAsync()) return;
            WindowNotificationManager?.Show(new Notification("Project saved", null, NotificationType.Success));
        }
        catch (Exception ex)
        {
            await _dialogueProvider.AlertAsync(AlertType.Warning,
                "Failed to save project",
                $"The project could not be saved. {ex.Message}");
        }
    }

    private async Task<bool> SaveProjectAsCoreAsync()
    {
        if (AssertInDesignMode()) return false;
        Debug.Assert(CurrentProjectProvider.ProjectContext != null);

        var window = _windowProvider.ProvideWindow();
        var storageFile = await window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Project",
            FileTypeChoices = [
                new FilePickerFileType("PubDoomer data file (binary)") { Patterns =  [$"*.{ProjectContext.ProjectBinaryFormatExtension}"] },
                new FilePickerFileType("PubDoomer data file (text)") { Patterns =  [$"*.{ProjectContext.ProjectTextFormatExtension}"] }
            ]
        });

        if (storageFile == null) return false;

        if (storageFile.TryGetLocalPath() is not { } filePath)
        {
            await _dialogueProvider.AlertAsync(AlertType.Warning,
                "Failed to save project",
                "The path to save is not a valid path.");
            return false;
        }
        
        // Verify the extension can be determined.
        if (Path.GetExtension(filePath) is not { } extension)
        {
            await _dialogueProvider.AlertAsync(AlertType.Warning,
                "Failed to save project",
                "The path to save is not a valid path.");
            return false;
        }

        // Determine the save type based on the existing extension.
        var type = extension[1..] switch
        {
            ProjectContext.ProjectBinaryFormatExtension => ProjectSaveType.Binary,
            ProjectContext.ProjectTextFormatExtension => ProjectSaveType.Text,
            _ => throw new ArgumentException($"Project type not found from extension: {extension}"),
        };

        var folder = Path.GetDirectoryName(filePath);
        if (folder == null)
        {
            await _dialogueProvider.AlertAsync(AlertType.Warning,
                "Failed to save project",
                "The path to save is not a valid directory.");
            return false;
        }
        
        CurrentProjectProvider.ProjectContext.SaveType = type;
        CurrentProjectProvider.ProjectContext.FolderPath = folder;
        CurrentProjectProvider.ProjectContext.FileName = Path.GetFileNameWithoutExtension(filePath);
        
        await using var writeStream = await storageFile.OpenWriteAsync();
        _savingService.SaveProject(CurrentProjectProvider.ProjectContext, writeStream);
        return true;
    }

    [RelayCommand]
    private async Task LoadProjectAsync()
    {
        if (AssertInDesignMode()) return;

        var window = _windowProvider.ProvideWindow();
        var storageFiles = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Project",
            AllowMultiple = false,
            FileTypeFilter = [
                new FilePickerFileType("PubDoomer data file (binary)") { Patterns = [$"*.{ProjectContext.ProjectBinaryFormatExtension}"] },
                new FilePickerFileType("PubDoomer data file (text)") { Patterns = [$"*.{ProjectContext.ProjectTextFormatExtension}"] }
            ]
        });

        if (storageFiles.Count == 0) return;

        var file = storageFiles.First();
        var filePath = file.Path.AbsolutePath;
        await using var fileStream = await file.OpenReadAsync();

        try
        {
            await TryLoadProjectPathAsync(filePath, fileStream);
            await UpdateRecentProjectsWithCurrentAsync();
        }
        catch (Exception ex)
        {
            await _dialogueProvider.AlertAsync(AlertType.Error, "The project could not be loaded.", ex.Message);
        }
    }

    [RelayCommand]
    private async Task OpenRecentProjectAsync()
    {
        if (RecentProjectIndex == -1) return;
        await OpenRecentProjectAsync(RecentProjects[RecentProjectIndex]);
    }
    
    /// <summary>
    /// Command prompts removal of the specified recent project and removes it when continuing.
    /// </summary>
    [RelayCommand]
    private async Task PromptRemoveRecentProjectAsync(RecentProject recentProject)
    {
        // In design mode we do not prompt, but rather remove immediately.
        if (AssertInDesignMode())
        {
            await RemoveRecentProjectAsync(recentProject);
            return;
        }
        
        var result = await _dialogueProvider.PromptAsync(
            AlertType.None,
            "Remove project",
            "Are you sure you want to remove the project from the list?",
            string.Empty,
            new InformationalWindowButton(AlertType.None, "Cancel"),
            new InformationalWindowButton(AlertType.Error, "Remove"));

        if (!result) return;
        await RemoveRecentProjectAsync(recentProject);
    }

    private async Task OpenRecentProjectAsync(RecentProject recentProject)
    {
        if (AssertInDesignMode()) return;

        // No project on the given path. Prompt to remove it.
        if (!File.Exists(recentProject.FilePath))
        {
            var result = await _dialogueProvider.PromptAsync(
                AlertType.None,
                "Failed to open project",
                "The project under the given path no longer exists.",
                "Would you like to remove it?",
                new InformationalWindowButton(AlertType.None, "Cancel"),
                new InformationalWindowButton(AlertType.Error, "Remove"));

            if (!result) return;
            await RemoveRecentProjectAsync(recentProject);
            return;
        }

        await using var fileStream = File.OpenRead(recentProject.FilePath);

        try
        {
            await TryLoadProjectPathAsync(recentProject.FilePath, fileStream);
        }
        catch (Exception ex)
        {
            var result = await _dialogueProvider.PromptAsync(
                AlertType.None,
                ex,
                "Failed to open project",
                "The project could not be loaded.",
                "Would you like to remove it?",
                new InformationalWindowButton(AlertType.None, "Cancel"),
                new InformationalWindowButton(AlertType.Error, "Remove"));

            if (!result) return;
            await RemoveRecentProjectAsync(recentProject);
        }
    }

    private async Task TryLoadProjectPathAsync(string filePath, Stream fileStream)
    {
        if (AssertInDesignMode()) return;
        
        var type = Path.GetExtension(filePath) switch
        {
            $".{ProjectContext.ProjectBinaryFormatExtension}" => ProjectSaveType.Binary,
            $".{ProjectContext.ProjectTextFormatExtension}" => ProjectSaveType.Text,
            _ => throw new ArgumentException($"Project type could not be determined from file '{filePath}'."),
        };
        
        var folderPath = Path.GetDirectoryName(filePath);
        if (folderPath == null)
        {
            await _dialogueProvider.AlertAsync(AlertType.Warning,
                "Failed to load project",
                "The path to load is not a valid directory.");
            return;
        }

        var projectContext = _savingService.LoadProject(folderPath, fileStream, type);
        
        projectContext.SaveType = type;
        projectContext.FolderPath = folderPath;
        projectContext.FileName = Path.GetFileNameWithoutExtension(filePath);
        
        CurrentProjectProvider.ProjectContext = projectContext;
    }

    // Loads all local data.
    private async Task LoadLocalDataAsync()
    {
        if (AssertInDesignMode()) return;

        try
        {
            await _recentProjectsService.LoadRecentProjectsAsync();
        }
        catch (Exception ex)
        {
            await _dialogueProvider.AlertAsync(AlertType.Warning, 
                ex,"The recent projects could not be loaded.");
        }

        try
        {
            await _localSettingsService.LoadLocalSettingsAsyncAsync();
        }
        catch (Exception ex)
        {
            await _dialogueProvider.AlertAsync(AlertType.Warning, 
                ex,"The local settings could not be loaded.");
        }
    }

    private async Task UpdateRecentProjectsWithCurrentAsync()
    {
        if (AssertInDesignMode()) return;

        if (CurrentProjectProvider.ProjectContext == null)
        {
            await _dialogueProvider.AlertAsync(AlertType.Warning,
                "The local settings could not be updated.", "Failed to retrieve current project.");
            return;
        }

        var fullPath = CurrentProjectProvider.ProjectContext.GetFullPath();

        // Remove this project from the recent projects if it exists.
        // Then, add it as the first recent project.
        var recentProject = RecentProjects.SingleOrDefault(x =>
            x.FilePath.Equals(fullPath, StringComparison.OrdinalIgnoreCase));

        if (recentProject != null) RecentProjects.Remove(recentProject);

        recentProject = new RecentProject(CurrentProjectProvider.ProjectContext.Name, fullPath);
        RecentProjects.Insert(0, recentProject);

        await _recentProjectsService.SaveRecentProjectsAsync();
    }
    
    /// <summary>
    /// Removes the given project form the list of recent projects and saves the current list.
    /// </summary>
    private async Task RemoveRecentProjectAsync(RecentProject recentProject)
    {
        RecentProjects.Remove(recentProject);
        
        // We do not save the recent projects in design mode.
        if (!AssertInDesignMode()) await _recentProjectsService.SaveRecentProjectsAsync();
    }

    [MemberNotNullWhen(false, nameof(_windowProvider), nameof(_savingService), nameof(_localSettingsService), nameof(_recentProjectsService), nameof(_dialogueProvider))]
    private bool AssertInDesignMode()
    {
        return Design.IsDesignMode;
    }
}