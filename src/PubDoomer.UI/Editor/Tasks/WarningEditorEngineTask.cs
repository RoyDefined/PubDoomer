using System;
using System.Collections.Generic;
using System.IO;
using PubDoomer.Engine.TaskInvokation.Task;
using PubDoomer.Engine.TaskInvokation.Validation;

namespace PubDoomer.UI.Editor.Tasks;

// Represents a task that will always return two warnings.
public sealed class WarningEditorEngineTask : IRunnableTask
{
    private static readonly Type HandlerTypeCached = typeof(WarningEditorEngineTaskHandler);

    public Type HandlerType => HandlerTypeCached;
    public required string Name { get; init; }

    public IEnumerable<ValidateResult> Validate()
    {
        yield return ValidateResult.FromWarning("Warning number #1");
        yield return ValidateResult.FromWarning("Warning number #2");
    }
}