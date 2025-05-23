﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PubDoomer.Project;
using PubDoomer.Project.Profile;
using PubDoomer.Project.Run;
using PubDoomer.Saving;
using PubDoomer.Services;
using PubDoomer.ViewModels.Dialogues;
using PubDoomer.Settings.Merged;
using System.Runtime;
using PubDoomer.Utils.TaskInvokation;
using PubDoomer.Engine.TaskInvokation.Orchestration;
using PubDoomer.Engine.TaskInvokation.Validation;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using System.IO;
using System.Runtime.InteropServices;
using PubDoomer.Engine.TaskInvokation.Context;

namespace PubDoomer.ViewModels.Pages;

public partial class ProfilesPageViewModel : PageViewModel
{
    private readonly ILogger _logger;
    private readonly ProjectTaskOrchestrator? _projectTaskOrchestrator;
    private readonly DialogueProvider? _dialogueProvider;
    private readonly WindowNotificationManager? _notificationManager;
    private readonly WindowProvider? _windowProvider;

    // The currently selected profile.
    [ObservableProperty] private ProfileContext? _selectedProfile;

    // The actual profile displayed in the UI, which includes additional information.
    [ObservableProperty] private ProfileRunContext? _selectedRunProfile;
    
    // Deferred properties from the selected run profile.
    [ObservableProperty] private ObservableCollection<TaskValidationCollection> _warnings = [];
    [ObservableProperty] private ObservableCollection<TaskValidationCollection> _errors = [];
    
    // This is the context used by the application when invoking a profile.
    private TaskInvokeContext? _taskInvokeContext;

    public ProfilesPageViewModel()
    {
        if (!Design.IsDesignMode) throw new InvalidOperationException();

        _logger = NullLogger.Instance;
        SessionSettings = new SessionSettings();
        CurrentProjectProvider = new CurrentProjectProvider();
        Settings = new LocalSettings();
        
        // Immediately set the selected profile so the UI shows the main scenario
        SelectedProfile = CurrentProjectProvider.ProjectContext!.Profiles[0];
    }

    public ProfilesPageViewModel(
        ILogger<ProfilesPageViewModel> logger,
        CurrentProjectProvider currentProjectProvider,
        SessionSettings sessionSettings,
        WindowNotificationManager notificationManager,
        WindowProvider windowProvider,
        DialogueProvider dialogueProvider,
        ProjectTaskOrchestrator projectTaskOrchestrator,
        LocalSettings settings)
    {
        _logger = logger;
        CurrentProjectProvider = currentProjectProvider;
        SessionSettings = sessionSettings;
        _notificationManager = notificationManager;
        _windowProvider = windowProvider;
        _dialogueProvider = dialogueProvider;
        _projectTaskOrchestrator = projectTaskOrchestrator;
        Settings = settings;

        _logger.LogDebug("Created.");
    }
    
    public CurrentProjectProvider CurrentProjectProvider { get; }
    public SessionSettings SessionSettings { get; }
    public LocalSettings Settings { get; }

    [RelayCommand]
    private async Task ExecuteProfileAsync()
    {
        if (AssertInDesignMode()) return;
        Debug.Assert(SelectedRunProfile != null);
        Debug.Assert(_taskInvokeContext != null);

        _logger.LogDebug("Executing profile {ProfileName}", SelectedRunProfile.Name);
        await _projectTaskOrchestrator.InvokeProfileAsync(SelectedRunProfile, _taskInvokeContext);
        _logger.LogDebug("Finished execution.");
    }

    private IEnumerable<TaskValidationCollection> GetValidationsByType(
        IEnumerable<TaskValidationCollection> validations,
        ValidateResultType type)
    {
        foreach (var validation in validations)
        {
            var results = validation.Results.Where(y => y.Type == type).ToArray();
            if (results.Length == 0)
            {
                continue;
            }
            
            yield return new TaskValidationCollection(validation.Task, results);
        }
    }

    [RelayCommand]
    private async Task CreateProfileAsync()
    {
        if (AssertInDesignMode()) return;
        Debug.Assert(CurrentProjectProvider.ProjectContext != null);

        var vm = new CreateOrEditProfileWindowViewModel(CurrentProjectProvider.ProjectContext);
        var result = await _dialogueProvider.GetCreateOrEditDialogueWindowAsync(vm);
        if (!result) return;

        Debug.Assert(CurrentProjectProvider.ProjectContext != null);

        CurrentProjectProvider.ProjectContext.Profiles.Add(vm.CurrentProfileContext);
        _notificationManager?.Show(new Notification("Profile created", "The profile has been created successfully.",
            NotificationType.Success));
    }

    [RelayCommand]
    private async Task EditProfileAsync(ProfileContext profileContext)
    {
        if (AssertInDesignMode()) return;
        Debug.Assert(CurrentProjectProvider.ProjectContext != null);

        var vm = new CreateOrEditProfileWindowViewModel(CurrentProjectProvider.ProjectContext, profileContext.DeepClone());
        var result = await _dialogueProvider.GetCreateOrEditDialogueWindowAsync(vm);
        if (!result) return;

        var index = CurrentProjectProvider.ProjectContext.Profiles.IndexOf(profileContext);
        CurrentProjectProvider.ProjectContext.Profiles[index] = vm.CurrentProfileContext;
        _notificationManager?.Show(new Notification("Profile edited", "The profile has been edited successfully.",
            NotificationType.Success));
    }

    [RelayCommand]
    private async Task DeleteProfileAsync(ProfileContext profileContext)
    {
        Debug.Assert(CurrentProjectProvider.ProjectContext != null);
        
        // In design mode we delete without a prompt.
        if (AssertInDesignMode())
        {
            CurrentProjectProvider.ProjectContext.Profiles.Remove(profileContext);
            return;
        }

        var result = await _dialogueProvider.PromptAsync(
            AlertType.Warning,
            "Delete profile",
            "Are you sure you want to delete this profile?",
            "The profile will be deleted and you will have to recreate it.",
            new InformationalWindowButton(AlertType.None, "Cancel"),
            new InformationalWindowButton(AlertType.Error, "Delete"));

        if (!result) return;

        CurrentProjectProvider.ProjectContext.Profiles.Remove(profileContext);
        _notificationManager?.Show(new Notification("Profile deleted", "The profile has been deleted.",
            NotificationType.Success));
    }

    // TODO: Check for a build in alternative.
    [RelayCommand]
    private async Task OpenWorkingDirectoryAsync()
    {
        if (AssertInDesignMode()) return;

        var folder = _taskInvokeContext?.WorkingDirectory ?? CurrentProjectProvider.ProjectContext?.FolderPath;
        if (folder == null)
        {
            await _dialogueProvider.AlertAsync(
                AlertType.Error, "Failed to open folder",
                "The system could not find the correct folder.");
            return;
        }

        if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
            return;

        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var info = new ProcessStartInfo("cmd", $"/c start {folder}")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
            
                Process.Start(info);
            }
            else
            {
                throw new PlatformNotSupportedException("This platform is not supported for this operation.");
            }
        }
        catch (Exception ex)
        {
            await _dialogueProvider.AlertAsync(
                AlertType.Error, ex, "Failed to open folder",
                "The system could not open the working directory.");
        }
    }

    partial void OnSelectedProfileChanged(ProfileContext? value)
    {
        if (AssertInDesignMode()) return;

        Debug.Assert(CurrentProjectProvider.ProjectContext != null);

        if (value == null)
        {
            return;
        }

        SelectedRunProfile = value.ToProfileRunContext();

        var settings = SettingsMerger.Merge(CurrentProjectProvider.ProjectContext, Settings);
        _taskInvokeContext = TaskInvokeContextUtil.BuildContext(CurrentProjectProvider.ProjectContext.FolderPath, settings);

        // Validate the profile before running it.
        // The UI will display the errors.
        var validations = _projectTaskOrchestrator.ValidateProfile(SelectedRunProfile, _taskInvokeContext);

        Warnings = new ObservableCollection<TaskValidationCollection>(GetValidationsByType(validations, ValidateResultType.Warning));
        Errors = new ObservableCollection<TaskValidationCollection>(GetValidationsByType(validations, ValidateResultType.Error));
    }

    [MemberNotNullWhen(false, nameof(_windowProvider), nameof(_dialogueProvider), nameof(_projectTaskOrchestrator))]
    private bool AssertInDesignMode()
    {
        return Design.IsDesignMode;
    }
}