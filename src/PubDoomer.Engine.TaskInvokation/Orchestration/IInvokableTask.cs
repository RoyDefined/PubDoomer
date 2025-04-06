using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using PubDoomer.Engine.TaskInvokation.Validation;
using System.Collections.ObjectModel;

namespace PubDoomer.Engine.TaskInvokation.Orchestration;

public interface IInvokableTask
{
    IRunnableTask Task { get; set; }
    ProfileTaskErrorBehaviour Behaviour { get; set; }

    ProfileRunTaskStatus Status { get; set; }
    ObservableCollection<TaskOutputResult> TaskOutput { get; set; }
}