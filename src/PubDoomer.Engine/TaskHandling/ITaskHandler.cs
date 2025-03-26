using PubDoomer.Engine.TaskHandling;

namespace PubDoomer.Engine.Orchestration;

public interface ITaskHandler
{
    ValueTask<TaskInvokationResult> HandleAsync();
}