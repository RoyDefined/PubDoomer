using System;
using System.Collections.Generic;
using System.IO;
using PubDoomer.Engine.Orchestration;
using PubDoomer.Engine.Validation;

namespace PubDoomer.UI.Editor.Tasks;

// Represents a task that will always return two warnings.
public sealed class WarningEditorEngineTask : EngineTaskBase
{
    private static readonly Type HandlerTypeCached = typeof(WarningEditorEngineTaskHandler);

    public override Type HandlerType => HandlerTypeCached;

    public override IEnumerable<ValidateResult> Validate()
    {
        yield return ValidateResult.FromWarning("Warning number #1");
        yield return ValidateResult.FromWarning("Warning number #2");
    }
}