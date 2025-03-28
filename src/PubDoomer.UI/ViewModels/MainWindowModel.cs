using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
        
        // Ensure that `ShowJsonFeatures` updates in the event the session settings get updated in any way.
        // It relies on the edit mode boolean.
        SessionSettings.PropertyChanged += (_, _) => OnPropertyChanged(nameof(ShowJsonFeatures));

        // Load local data in the background.
        _ = Task.Run(LoadLocalDataAsync);
    }

    public bool WindowEnlarged => _windowProvider?.TryProvideWindow(out var window) == true
                                  && window.WindowState == WindowState.Maximized;

    public bool ShowJsonFeatures => SessionSettings.EnableEditing && Environment?.IsDevelopment == true;

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
        WindowNotificationManager?.Show(new Notification("Project created", "The project has been created succesfully.",
            NotificationType.Success));
    }

    [RelayCommand]
    private async Task SaveProjectAsync()
    {
        await SaveProjectAsync(true);
    }
    
    [RelayCommand]
    private async Task SaveProjectJsonAsync()
    {
        await SaveProjectAsync(false);
    }

    private async Task SaveProjectAsync(bool encrypt)
    {
        if (AssertInDesignMode()) return;
        Debug.Assert(CurrentProjectProvider.ProjectContext != null);

        await _savingService.SaveProjectAsync(encrypt);
        WindowNotificationManager?.Show(new Notification("Project saved", null, NotificationType.Success));
    }

    [RelayCommand]
    private async Task SaveProjectAsAsync()
    {
        await SaveProjectAsAsync(true);
    }
    
    [RelayCommand]
    private async Task SaveProjectJsonAsAsync()
    {
        await SaveProjectAsAsync(false);
    }
    
    private async Task SaveProjectAsAsync(bool encrypt)
    {
        if (AssertInDesignMode()) return;
        Debug.Assert(CurrentProjectProvider.ProjectContext != null);

        var window = _windowProvider.ProvideWindow();
        var storageFile = await window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            DefaultExtension = encrypt ? "dat" : "json",
            Title = "Save Project"
        });

        if (storageFile == null) return;

        // Set the new path, which is then used to save the project.
        CurrentProjectProvider.ProjectContext.FilePath = storageFile.Path;

        await _savingService.SaveProjectAsync(encrypt);
        WindowNotificationManager?.Show(new Notification("Project saved", null, NotificationType.Success));
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
                new FilePickerFileType("Binary data") { Patterns = ["*.dat"] },
                new FilePickerFileType("JSON format") { Patterns = ["*.json"] }
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

        try
        {
            await _savingService.LoadProjectOrDefaultAsync(projectPath);
            
            // If still null, the project no longer existed, probably.
            if (CurrentProjectProvider.ProjectContext == null)
            {
                await _dialogueProvider.AlertAsync(AlertType.Warning,
                    "Failed to open project",
                    "The project under the given path no longer exists.");
                return;
            }
        }
        catch (JsonException ex)
        {
            Debug.Fail($"The project could not be loaded. {ex.Message}");
            await _dialogueProvider.AlertAsync(AlertType.Error, "The project could not be loaded.");
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
        catch (Exception e)
        {
            Debug.Fail("The recent projects could not be loaded.");
            await _dialogueProvider.AlertAsync(AlertType.Warning, "The recent projects could not be loaded.");
        }

        try
        {
            await _localSettingsService.LoadLocalSettingsAsyncAsync();
        }
        catch (Exception e)
        {
            Debug.Fail("The local settings could not be loaded.");
            await _dialogueProvider.AlertAsync(AlertType.Warning, "The local settings could not be loaded.");
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
            x.FilePath.Equals(CurrentProjectProvider.ProjectContext.FilePath.LocalPath, StringComparison.OrdinalIgnoreCase));

        if (recentProject != null) RecentProjects.Remove(recentProject);

        recentProject = new RecentProject(CurrentProjectProvider.ProjectContext.Name!, CurrentProjectProvider.ProjectContext.FilePath.LocalPath);
        RecentProjects.Insert(0, recentProject);

        await _recentProjectsService.SaveRecentProjectsAsync();
    }

    [MemberNotNullWhen(false, nameof(_windowProvider), nameof(_savingService), nameof(_localSettingsService), nameof(_recentProjectsService), nameof(_dialogueProvider))]
    private bool AssertInDesignMode()
    {
        return Design.IsDesignMode;
    }
}