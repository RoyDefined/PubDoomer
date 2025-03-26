using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PubDoomer.Engine.Orchestration;
using PubDoomer.Engine.TaskHandlers;
using PubDoomer.Engine.Validation;

namespace PubDoomer.Engine.Tasks;

public sealed class AcsVirtualMachineExecuteTask : EngineTaskBase
{
    private static readonly Type HandlerTypeCached = typeof(AcsVirtualMachineExecuteTaskHandler);
    public override Type HandlerType => HandlerTypeCached;

    public required string InputFilePath { get; init; }

    // TODO: Implement. Current not implemented as this task is part of automatically generated tasks for the code editor page.
    public override IEnumerable<ValidateResult> Validate()
    {
        yield break;
    }
}
