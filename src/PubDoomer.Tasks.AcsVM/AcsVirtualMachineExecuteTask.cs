using PubDoomer.Engine.TaskInvokation.Task;
using PubDoomer.Engine.TaskInvokation.Validation;

namespace PubDoomer.Engine.Tasks.AcsVM;

public sealed class AcsVirtualMachineExecuteTask : IRunnableTask
{
    private static readonly Type HandlerTypeCached = typeof(AcsVirtualMachineExecuteTaskHandler);
    public Type HandlerType => HandlerTypeCached;

    public required string Name { get; init; }
    public required string InputFilePath { get; init; }

    // TODO: Implement. Current not implemented as this task is part of automatically generated tasks for the code editor page.
    public IEnumerable<ValidateResult> Validate()
    {
        yield break;
    }
}
