namespace PubDoomer.Engine.TaskInvokation.Task;

public interface ITaskHandler
{
    ValueTask<TaskInvokationResult> HandleAsync();
}