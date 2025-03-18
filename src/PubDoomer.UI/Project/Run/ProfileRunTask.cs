using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Engine.Orchestration;
using PubDoomer.Project.Profile;
using PubDoomer.Project.Tasks;

namespace PubDoomer.Project.Run;

/// <summary>
/// Represents the main context of a task to run.
/// <br /> Contains additional fields and properties to indicate result.
/// </summary>
public partial class ProfileRunTask : ObservableObject
{
    [ObservableProperty] private EngineTaskBase _task;
    [ObservableProperty] private ProfileTaskErrorBehaviour _behaviour;

    [ObservableProperty] private ObservableCollection<ValidateResult>? _validations;

    [ObservableProperty] private ProfileRunTaskStatus _status;
    [ObservableProperty] private string? _resultMessage;
    [ObservableProperty] private ObservableCollection<string>? _resultWarnings;
    [ObservableProperty] private ObservableCollection<string>? _resultErrors;
    [ObservableProperty] private Exception? _exception;

    public ProfileRunTask(
        ProfileTaskErrorBehaviour behaviour,
        EngineTaskBase task)
    {
        _behaviour = behaviour;
        _task = task;

        _status = ProfileRunTaskStatus.Pending;
    }
}