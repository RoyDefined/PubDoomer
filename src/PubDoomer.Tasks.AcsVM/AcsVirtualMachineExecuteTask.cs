using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using PubDoomer.Engine.TaskInvokation.Validation;

namespace PubDoomer.Tasks.AcsVM;

public sealed class AcsVirtualMachineExecuteTask : IRunnableTask
{
    public Type HandlerType => typeof(AcsVirtualMachineExecuteTaskHandler);
    public Type? ValidatorType => null;

    public required string Name { get; init; }
    public required string InputFilePath { get; init; }

}
