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
    private const string ProjectBinaryFormatExtension = ".pdbproj";
    
    private readonly DialogueProvider? _dialogueProvider;
    private readonly ProjectSavingService? _savingService;
    private readonly LocalSettingsService? _localSettingsService;
    private readonly RecentProjectsService? _recentProjectsService;
    private readonly WindowProvider? _windowProvider;

    [ObservableProperty] private RecentProjectCollection _recentProjects;

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

        var vm = new CreateOrEditProjectWindowViewModel();
        var result = await _dialogueProvider.GetCreateOrEditDialogueWindowAsync(vm);
        if (!result) return;

        CurrentProjectProvider.ProjectContext = vm.Project;
        WindowNotificationManager?.Show(new Notification("Project created", "The project has been created successfully.",
            NotificationType.Success));
    }

    [RelayCommand]
    private async Task SaveProjectAsync()
    {
        if (AssertInDesignMode()) return;
        
        var projectContext = CurrentProjectProvider.ProjectContext;
        if (projectContext == null)
        {
            return;
        }
        
        // 'Save as' in case of no file path.
        if (projectContext.FilePath == null)
        {
            await SaveProjectAsAsync();
            return;
        }

        // TODO: Allow different formats.
        using var fileStream = File.OpenWrite(projectContext.FilePath);
        _savingService.SaveProject(projectContext, projectContext.FilePath, fileStream, ProjectReadingWritingType.Binary);
        WindowNotificationManager?.Show(new Notification("Project saved", null, NotificationType.Success));
    }

    [RelayCommand]
    private async Task SaveProjectAsAsync()
    {
        if (AssertInDesignMode()) return;
        Debug.Assert(CurrentProjectProvider.ProjectContext != null);

        var window = _windowProvider.ProvideWindow();
        var storageFile = await window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            DefaultExtension = ProjectBinaryFormatExtension,
            Title = "Save Project"
        });

        if (storageFile == null) return;

        if (storageFile.TryGetLocalPath() is not string filePath)
        {
            await _dialogueProvider.AlertAsync(AlertType.Warning,
                "Failed to use path",
                "The path to save is not a valid path.");
            return;
        }

        // TODO: Allow different formats.
        var writeStream = await storageFile.OpenWriteAsync();
        _savingService.SaveProject(CurrentProjectProvider.ProjectContext, filePath, writeStream, ProjectReadingWritingType.Binary);
        WindowNotificationManager?.Show(new Notification("Project saved", null, NotificationType.Success));
        CurrentProjectProvider.ProjectContext.FilePath = filePath;
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
                new FilePickerFileType("PubDoomer data format") { Patterns = [$"*.{ProjectBinaryFormatExtension}"] }
            ]
        });

        if (storageFiles.Count == 0) return;

        await TryLoadProjectPathAsync(storageFiles.Single().Path.AbsolutePath);
        await UpdateRecentProjectsWithCurrentAsync();
    }

    [RelayCommand]
    private async Task OpenRecentProjectAsync(string filePath)
    {
        await TryLoadProjectPathAsync(filePath);
    }

    private async Task TryLoadProjectPathAsync(string projectPath)
    {
        if (AssertInDesignMode()) return;

        // No project on the given path.
        // TODO: Remove project from recent projects if it was retrieved from there.
        if (!File.Exists(projectPath))
        {
            await _dialogueProvider.AlertAsync(AlertType.Warning,
                "Failed to open project",
                "The project under the given path no longer exists.");
            return;
        }
        
        try
        {
            // TODO: Allow different formats.
            var projectContext = _savingService.LoadProject(projectPath, ProjectReadingWritingType.Binary);
            CurrentProjectProvider.ProjectContext = projectContext;
        }
        catch (Exception ex)
        {
            await _dialogueProvider.AlertAsync(AlertType.Error, $"The project could not be loaded. {ex.Message}");
        }
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
            await _dialogueProvider.AlertAsync(AlertType.Warning, $"The recent projects could not be loaded. {ex.Message}");
        }

        try
        {
            await _localSettingsService.LoadLocalSettingsAsyncAsync();
        }
        catch (Exception ex)
        {
            await _dialogueProvider.AlertAsync(AlertType.Warning, $"The local settings could not be loaded. {ex.Message}");
        }
    }

    private async Task UpdateRecentProjectsWithCurrentAsync()
    {
        if (AssertInDesignMode()) return;

        if (CurrentProjectProvider?.ProjectContext?.FilePath == null)
            return;

        // Remove this project from the recent projects if it exists.
        // Then, add it as the first recent project.
        var recentProject = RecentProjects.SingleOrDefault(x =>
            x.FilePath.Equals(CurrentProjectProvider.ProjectContext.FilePath, StringComparison.OrdinalIgnoreCase));

        if (recentProject != null) RecentProjects.Remove(recentProject);

        recentProject = new RecentProject(CurrentProjectProvider.ProjectContext.Name!, CurrentProjectProvider.ProjectContext.FilePath);
        RecentProjects.Insert(0, recentProject);

        await _recentProjectsService.SaveRecentProjectsAsync();
    }

    [MemberNotNullWhen(false, nameof(_windowProvider), nameof(_savingService), nameof(_localSettingsService), nameof(_recentProjectsService), nameof(_dialogueProvider))]
    private bool AssertInDesignMode()
    {
        return Design.IsDesignMode;
    }
}