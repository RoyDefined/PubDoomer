namespace PubDoomer.Engine.Orchestration;

public interface ITaskHandler
{
    ValueTask<TaskInvokationResult> HandleAsync();
}