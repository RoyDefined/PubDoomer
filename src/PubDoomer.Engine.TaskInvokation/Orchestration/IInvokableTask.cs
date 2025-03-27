using PubDoomer.Engine.TaskInvokation.Orchestration;
using PubDoomer.Engine.TaskInvokation.Validation;
using System.Collections.ObjectModel;

namespace PubDoomer.Engine.TaskInvokation.Task;

public interface IInvokableTask
{
    IRunnableTask Task { get; set; }
    ProfileTaskErrorBehaviour Behaviour { get; set; }

    ProfileRunTaskStatus Status { get; set; }
    string? ResultMessage { get; set; }
    ObservableCollection<string>? ResultWarnings { get; set; }
    ObservableCollection<string>? ResultErrors { get; set; }
    Exception? Exception { get; set; }
}