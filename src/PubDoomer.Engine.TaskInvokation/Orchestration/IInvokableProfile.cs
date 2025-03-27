using PubDoomer.Engine.TaskInvokation.Orchestration;
using PubDoomer.Engine.TaskInvokation.Validation;
using System.Collections.ObjectModel;

namespace PubDoomer.Engine.TaskInvokation.Task;

public interface IInvokableProfile
{
    IEnumerable<IInvokableTask> Tasks { get; }
    ProfileRunContextStatus Status { get; set; }
    int? ElapsedTimeMs { get; set; }
}