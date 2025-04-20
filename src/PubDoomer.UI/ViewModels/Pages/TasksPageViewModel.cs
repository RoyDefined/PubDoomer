using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PubDoomer.Project;
using PubDoomer.Project.Tasks;
using PubDoomer.Saving;
using PubDoomer.Services;
using PubDoomer.Tasks.Compile.Acc;
using PubDoomer.Tasks.Compile.Bcc;
using PubDoomer.Tasks.Compile.GdccAcc;
using PubDoomer.ViewModels.Dialogues;

namespace PubDoomer.ViewModels.Pages;

public partial class TasksPageViewModel : PageViewModel
{
    private readonly DialogueProvider? _dialogueProvider;
    private readonly ILogger _logger;
    private readonly WindowNotificationManager? _notificationManager;
    private readonly WindowProvider? _windowProvider;

    public TasksPageViewModel()
    {
        if (!Design.IsDesignMode) throw new InvalidOperationException();

        _logger = NullLogger.Instance;
        SessionSettings = new SessionSettings();
        CurrentProjectProvider = new CurrentProjectProvider();
        
        // In this instance we replace the project's tasks to a predefined set, because otherwise we're just looking at designer tasks.
        var compileTask1 = new ObservableAccCompileTask("Compile Foo project task", "Path/To/Foo.acs", "Path/To/Foo.o");
        var compileTask2 = new ObservableBccCompileTask("Compile Bar BCC project task", "Path/To/Bar.acs", "Path/To/Bar.o");
        var compileTask3 = new ObservableGdccAccCompileTask("Compile Baz GDCC-ACC project task", "Path/To/Baz.bcs", "Path/To/Baz.o");
        CurrentProjectProvider.ProjectContext!.Tasks.Clear();
        CurrentProjectProvider.ProjectContext!.Tasks.Add(compileTask1);
        CurrentProjectProvider.ProjectContext!.Tasks.Add(compileTask2);
        CurrentProjectProvider.ProjectContext!.Tasks.Add(compileTask3);
    }

    public TasksPageViewModel(
        ILogger<TasksPageViewModel> logger,
        SessionSettings sessionSettings,
        CurrentProjectProvider currentProjectProvider,
        WindowNotificationManager notificationManager,
        WindowProvider windowProvider,
        DialogueProvider dialogueProvider)
    {
        _logger = logger;
        SessionSettings = sessionSettings;
        CurrentProjectProvider = currentProjectProvider;
        _notificationManager = notificationManager;
        _windowProvider = windowProvider;
        _dialogueProvider = dialogueProvider;

        _logger.LogDebug("Created.");
    }

    public SessionSettings SessionSettings { get; }
    public CurrentProjectProvider CurrentProjectProvider { get; }
    
    // Form data commands
    [RelayCommand]
    private async Task CreateTaskAsync()
    {
        if (AssertInDesignMode()) return;

        var vm = new CreateOrEditTaskWindowViewModel(_windowProvider, CurrentProjectProvider);
        var result = await _dialogueProvider.GetCreateOrEditDialogueWindowAsync(vm);
        if (!result) return;

        Debug.Assert(vm.CurrentTask != null);
        Debug.Assert(CurrentProjectProvider.ProjectContext != null);

        CurrentProjectProvider.ProjectContext.AddTask(vm.CurrentTask);
        _notificationManager?.Show(new Notification("Task created", "The task has been created succesfully.",
            NotificationType.Success));
    }

    [RelayCommand]
    private async Task EditTaskAsync(ProjectTaskBase task)
    {
        if (AssertInDesignMode()) return;
        Debug.Assert(CurrentProjectProvider.ProjectContext != null);

        var vm = new CreateOrEditTaskWindowViewModel(_windowProvider, CurrentProjectProvider, task.DeepClone());
        var result = await _dialogueProvider.GetCreateOrEditDialogueWindowAsync(vm);
        if (!result) return;

        var index = CurrentProjectProvider.ProjectContext.Tasks.IndexOf(task);
        CurrentProjectProvider.ProjectContext.Tasks[index].Merge(vm.CurrentTask!);
        _notificationManager?.Show(new Notification("Task edited", "The task has been edited succesfully.",
            NotificationType.Success));
    }

    [RelayCommand]
    private async Task DeleteTaskAsync(ProjectTaskBase task)
    {
        if (AssertInDesignMode()) return;
        Debug.Assert(CurrentProjectProvider.ProjectContext != null);

        var result = await _dialogueProvider.PromptAsync(
            AlertType.Warning,
            "Delete task",
            "Are you sure you want to delete this task?",
            "The task will be deleted and you will have to recreate it.",
            new InformationalWindowButton(AlertType.None, "Cancel"),
            new InformationalWindowButton(AlertType.Error, "Delete"));

        if (!result) return;
        CurrentProjectProvider.ProjectContext.RemoveTask(task);
        _notificationManager?.Show(new Notification("Task deleted", "The task has been deleted.",
            NotificationType.Success));
    }

    [MemberNotNullWhen(false, nameof(_windowProvider), nameof(_dialogueProvider))]
    private bool AssertInDesignMode()
    {
        return Design.IsDesignMode;
    }
}