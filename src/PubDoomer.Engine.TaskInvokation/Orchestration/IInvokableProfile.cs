using PubDoomer.Engine.TaskInvokation.Validation;
using System.Collections.ObjectModel;

namespace PubDoomer.Engine.TaskInvokation.Orchestration;

public interface IInvokableProfile
{
    IEnumerable<IInvokableTask> Tasks { get; }
    ProfileRunContextStatus Status { get; set; }
    int? ElapsedTimeMs { get; set; }
}