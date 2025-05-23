﻿using System;
using System.ComponentModel;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using PubDoomer.Project;
using PubDoomer.Project.Profile;
using PubDoomer.Project.Tasks;
using PubDoomer.Tasks.Compile.Acc;
using PubDoomer.Tasks.Compile.Bcc;

namespace PubDoomer.ViewModels.Dialogues;

public partial class CreateOrEditProfileWindowViewModel : ViewModelBase
{
    [ObservableProperty] private string _createOrEditButtonText;

    // The currently selected profile.
    [ObservableProperty] private ProfileContext _currentProfileContext;

    // The currently selected task from the dropdown.
    [ObservableProperty] private ProjectTaskBase? _currentTask;

    // The project to apply the profile to.
    [ObservableProperty] private ProjectContext _projectContext;

    // Form visuals
    [ObservableProperty] private string _windowTitle;

    public CreateOrEditProfileWindowViewModel()
    {
        if (!Design.IsDesignMode) throw new InvalidOperationException();

        ProjectContext = new ProjectContext
        {
            Name = "Project Foo"
        };

        CurrentProfileContext = new ProfileContext();

        WindowTitle = "Create new Profile";
        CreateOrEditButtonText = "Create";

        SubscribeProfileChanges();

        var compileTask1 = new ObservableAccCompileTask("Compile Foo project task", "Path/To/Foo.acs", "Path/To/Foo.o");
        var compileTask2 = new ObservableAccCompileTask("Compile Bar project task", "Path/To/Bar.acs", "Path/To/Bar.o");
        var compileTask3 = new ObservableBccCompileTask("Compile Baz BCC project task", "Path/To/Baz.bcs", "Path/To/Baz.o");

        var task1 = new ProfileTask
        {
            Behaviour = ProfileTaskErrorBehaviour.DontStop,
            Task = compileTask1
        };

        var task2 = new ProfileTask
        {
            Behaviour = ProfileTaskErrorBehaviour.StopOnError,
            Task = compileTask2
        };

        var task3 = new ProfileTask
        {
            Behaviour = ProfileTaskErrorBehaviour.StopOnError,
            Task = compileTask3
        };

        ProjectContext = new ProjectContext
        {
            Name = "Project Foo",
            Tasks = [compileTask1, compileTask2, compileTask3]
        };

        CurrentProfileContext = new ProfileContext
        {
            Name = "Compile whole project",
            Tasks = [task1, task2, task3]
        };
    }

    public CreateOrEditProfileWindowViewModel(
        ProjectContext projectContext)
    {
        ProjectContext = projectContext;
        CurrentProfileContext = new ProfileContext();

        WindowTitle = "Create new Profile";
        CreateOrEditButtonText = "Create";

        SubscribeProfileChanges();
    }

    public CreateOrEditProfileWindowViewModel(
        ProjectContext projectContext,
        ProfileContext profileContext)
    {
        ProjectContext = projectContext;
        CurrentProfileContext = profileContext;

        WindowTitle = "Edit profile";
        CreateOrEditButtonText = "Edit";

        SubscribeProfileChanges();
    }

    public bool FormIsValid => !string.IsNullOrWhiteSpace(CurrentProfileContext.Name);

    private void SubscribeProfileChanges()
    {
        CurrentProfileContext.PropertyChanged += UpdateFormValid;
    }

    private void UpdateFormValid(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(FormIsValid));
    }

    [RelayCommand]
    private void AddTask()
    {
        if (CurrentTask == null) return;

        var profileTask = new ProfileTask
        {
            Task = CurrentTask,
            Behaviour = ProfileTaskErrorBehaviour.StopOnError
        };

        CurrentProfileContext.Tasks.Add(profileTask);
        
        // INPC on the collection so the buttons update.
        CurrentProfileContext.RaiseTasksChanged();
    }

    [RelayCommand]
    private void RemoveTask(ProfileTask task)
    {
        CurrentProfileContext.Tasks.Remove(task);
        
        // INPC on the collection so the buttons update.
        CurrentProfileContext.RaiseTasksChanged();
    }
    
    [RelayCommand]
    private void MoveTaskUp(ProfileTask task)
    {
        var tasks = CurrentProfileContext.Tasks;
        var index = tasks.IndexOf(task);
        if (index > 0)
        {
            tasks.Move(index, index - 1);
        }
        
        // INPC on the collection so the buttons update.
        CurrentProfileContext.RaiseTasksChanged();
    }

    [RelayCommand]
    private void MoveTaskDown(ProfileTask task)
    {
        var tasks = CurrentProfileContext.Tasks;
        var index = tasks.IndexOf(task);
        if (index < tasks.Count - 1 && index >= 0)
        {
            tasks.Move(index, index + 1);
        }
        
        // INPC on the collection so the buttons update.
        CurrentProfileContext.RaiseTasksChanged();
    }
}