using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Engine.TaskInvokation.Task;
using PubDoomer.Project.Run;
using PubDoomer.Project.Tasks;

namespace PubDoomer.Project.Profile;

/// <summary>
/// Represents a configured task on a profile.
/// </summary>
public partial class ProfileTask : ObservableObject
{
    /// <summary>
    /// The behaviour of the task in the event of an error.
    /// </summary>
    [ObservableProperty] private ProfileTaskErrorBehaviour? _behaviour;
    
    /// <summary>
    /// The base task definition.
    /// </summary>
    [ObservableProperty] private ProjectTaskBase? _task;

    public ProfileRunTask ToRunnableTask()
    {
        Debug.Assert(Behaviour != null && Task != null);

        var engineTask = Task.ToEngineTaskBase();
        return new ProfileRunTask(Behaviour.Value, engineTask);
    }
}