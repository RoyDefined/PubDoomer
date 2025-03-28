using System;
using System.Collections.Generic;
using System.IO;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using PubDoomer.Engine.TaskInvokation.Validation;

namespace PubDoomer.UI.Editor.Tasks;

// Represents a task that will always complete succesfully.
public sealed class SuccesfulEditorEngineTask : IRunnableTask, IValidatableTask
{
    private static readonly Type HandlerTypeCached = typeof(SuccesfulEditorEngineTaskHandler);

    public Type HandlerType => HandlerTypeCached;
    public required string Name { get; init; }

    public IEnumerable<ValidateResult> Validate()
    {
        yield break;
    }
}