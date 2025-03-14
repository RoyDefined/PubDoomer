using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Project.Run;
using PubDoomer.Project.Tasks;

namespace PubDoomer.Project.Profile;

public partial class ProfileTask : ObservableObject
{
    [ObservableProperty] private ProfileTaskErrorBehaviour? _behaviour;
    [ObservableProperty] private ProjectTaskBase? _task;

    public ProfileRunTask ToRunnableTask()
    {
        Debug.Assert(Behaviour != null && Task != null);

        var engineTask = Task.ToEngineTaskBase();
        return new ProfileRunTask(Behaviour.Value, engineTask);
    }
}