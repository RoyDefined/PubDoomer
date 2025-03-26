using System;
using System.Collections.Generic;
using System.IO;
using PubDoomer.Engine.TaskHandling;
using PubDoomer.Engine.Validation;

namespace PubDoomer.UI.Editor.Tasks;

// Represents a task that will always return two errors.
public sealed class ErrorEditorEngineTask : EngineTaskBase
{
    private static readonly Type HandlerTypeCached = typeof(ErrorEditorEngineTaskHandler);

    public override Type HandlerType => HandlerTypeCached;

    public override IEnumerable<ValidateResult> Validate()
    {
        yield return ValidateResult.FromError("Error number #1");
        yield return ValidateResult.FromError("Error number #2");
    }
}