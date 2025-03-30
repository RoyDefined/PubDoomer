namespace PubDoomer.Engine.TaskInvokation.TaskDefinition;

public interface ITaskHandler
{
    ValueTask<TaskInvokationResult> HandleAsync();
}