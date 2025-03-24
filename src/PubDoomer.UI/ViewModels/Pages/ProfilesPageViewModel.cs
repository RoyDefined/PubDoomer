using System;
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
using PubDoomer.Engine.Orchestration;
using PubDoomer.Utils.MergedSettings;

namespace PubDoomer.ViewModels.Pages;

public partial class ProfilesPageViewModel : PageViewModel
{
    private readonly ILogger _logger;
    private readonly ProjectTaskOrchestrator _projectTaskOrchestrator;
    private readonly DialogueProvider? _dialogueProvider;
    private readonly WindowNotificationManager? _notificationManager;
    private readonly WindowProvider? _windowProvider;

    // The currently selected profile.
    [ObservableProperty] private ProfileContext? _selectedProfile;

    // The actual profile displayed in the UI, which includes additional information.
    [ObservableProperty]
    private ProfileRunContext? _selectedRunProfile;
    
    // Deferred properties from the selected run profile.
    [ObservableProperty] private ObservableCollection<TaskValidationCollection> _warnings = [];
    [ObservableProperty] private ObservableCollection<TaskValidationCollection> _errors = [];

    public ProfilesPageViewModel()
    {
        if (!Design.IsDesignMode) throw new InvalidOperationException();

        _logger = NullLogger.Instance;
        SessionSettings = new SessionSettings();
        CurrentProjectProvider = new CurrentProjectProvider();
        Settings = new LocalSettings();
        
        // Instantiate the task orchestrator under a basic `Activator.CreateInstance` call.
        // The tasks used in the designer do not require dependencies.
        // TODO: Duplicate code in the code editor
        _projectTaskOrchestrator = new ProjectTaskOrchestrator(
            NullLogger<ProjectTaskOrchestrator>.Instance,
            ((type, task, context) => Activator.CreateInstance(type, task, context) as ITaskHandler));
        
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
        // This method runs in the designer because a profile has test tasks.
        
        Debug.Assert(SelectedRunProfile != null);
        
        _logger.LogDebug("Executing profile {ProfileName}", SelectedRunProfile.Name);

        // Create the context to pass.
        var context = SettingsMerger.Merge(CurrentProjectProvider.ProjectContext, Settings);

        // TODO: Make use of the status.
        var stopwatch = Stopwatch.GetTimestamp();
        SelectedRunProfile.Status = ProfileRunContextStatus.Running;

        // TODO: Merge with the orchestrator.
        foreach (var runTask in SelectedRunProfile.Tasks)
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
            // If there was an error and the behaviour is to quit, then end the task invocation early.
            if (runTask.Status == ProfileRunTaskStatus.Error)
            {
                if (runTask.Behaviour == ProfileTaskErrorBehaviour.StopOnError)
                {
                    _logger.LogWarning(runTask.Exception, "Task failure.");
                    SelectedRunProfile.Status = ProfileRunContextStatus.Error;
                    break;
                }
                else
                {
                    _logger.LogWarning(runTask.Exception, "Task failed but is configured to not stop on errors. Execution will continue.");
                }
            }
        }

        SelectedRunProfile.ElapsedTimeMs = (int)Stopwatch.GetElapsedTime(stopwatch).TotalMilliseconds;
        if (SelectedRunProfile.Status != ProfileRunContextStatus.Error)
        {
            SelectedRunProfile.Status = ProfileRunContextStatus.Success;
        }
        
        _logger.LogDebug("Finished execution.");
    }

    private IEnumerable<TaskValidationCollection> GetValidationsByType(
        IEnumerable<TaskValidationCollection> validations,
        ValidateResultType type)
    {
        foreach (var validation in validations)
        {
            var results = validation.Results.Where(y => y.Type == type);
            var resultCollection = new Collection<ValidateResult>(results.ToArray());
            if (resultCollection.Count == 0)
            {
                continue;
            }
            
            yield return new TaskValidationCollection(validation.Task, resultCollection);
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

    partial void OnSelectedProfileChanged(ProfileContext? value)
    {
        if (value == null)
        {
            return;
        }

        SelectedRunProfile = value.ToProfileRunContext();
        
        // Validate the profile before running it.
        // If this results in validation errors, don't continue.
        // The UI will display the errors.
        SelectedRunProfile.ValidateContext();
        Debug.Assert(SelectedRunProfile?.Validations != null);

        Warnings = new ObservableCollection<TaskValidationCollection>(GetValidationsByType(SelectedRunProfile.Validations, ValidateResultType.Warning));
        Errors = new ObservableCollection<TaskValidationCollection>(GetValidationsByType(SelectedRunProfile.Validations, ValidateResultType.Error));
    }

    [MemberNotNullWhen(false, nameof(_windowProvider), nameof(_dialogueProvider))]
    private bool AssertInDesignMode()
    {
        return Design.IsDesignMode;
    }
}