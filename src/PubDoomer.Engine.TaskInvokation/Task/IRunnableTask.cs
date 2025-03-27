using PubDoomer.Engine.TaskInvokation.Validation;

namespace PubDoomer.Engine.TaskInvokation.Task;

public interface IRunnableTask
{
    Type HandlerType { get; }
    string? Name { get; }
}