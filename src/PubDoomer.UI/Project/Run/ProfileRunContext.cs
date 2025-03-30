using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Engine.TaskInvokation.Orchestration;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using PubDoomer.Engine.TaskInvokation.Validation;

namespace PubDoomer.Project.Run;

/// <summary>
/// Represents the main context of a profile that contains invokable tasks that can be invoked.
/// </summary>
public partial class ProfileRunContext : ObservableObject, IInvokableProfile
{
    /// <summary>
    /// The name of the profile to display in the UI.
    /// </summary>
    [ObservableProperty] private string _name;
    
    /// <summary>
    /// The tasks to be invoked in order.
    /// </summary>
    [ObservableProperty] private ObservableCollection<ProfileRunTask> _tasks;

    /// <summary>
    /// The status of the profile. Indicates how invokation goes.
    /// </summary>
    [ObservableProperty] private ProfileRunContextStatus _status;
    
    /// <summary>
    /// The total elapsed time that the profile ran for.
    /// <br /> If <c>null</c>, the profile has not run yet.
    /// </summary>
    [ObservableProperty] private int? _elapsedTimeMs;

    public ProfileRunContext(
        string name,
        IEnumerable<ProfileRunTask> tasks)
    {
        Name = name;
        Tasks = [.. tasks];
    }

    IEnumerable<IInvokableTask> IInvokableProfile.Tasks => Tasks.Cast<IInvokableTask>();
}