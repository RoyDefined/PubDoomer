using System;
using System.Collections.Generic;
using System.IO;
using PubDoomer.Engine.Orchestration;

namespace PubDoomer.UI.Editor.Tasks;

// Represents a task that will always complete succesfully.
public sealed class SuccesfulEditorEngineTask : EngineTaskBase
{
    private static readonly Type HandlerTypeCached = typeof(SuccesfulEditorEngineTaskHandler);

    public override Type HandlerType => HandlerTypeCached;

    public override IEnumerable<ValidateResult> Validate()
    {
        yield break;
    }
}