using PubDoomer.Engine.TaskInvokation.Validation;

namespace PubDoomer.Engine.TaskInvokation.TaskDefinition;

public interface IRunnableTask
{
    Type HandlerType { get; }
    string? Name { get; }
}