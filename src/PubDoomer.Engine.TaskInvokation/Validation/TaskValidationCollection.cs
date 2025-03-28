using PubDoomer.Engine.TaskInvokation.Orchestration;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PubDoomer.Engine.TaskInvokation.Validation;

/// <summary>
/// A collection that contains validation results of a given task.
/// </summary>
public sealed class TaskValidationCollection
{
    public TaskValidationCollection(
        IInvokableTask task,
        ValidateResult[] results)
    {
        Task = task;
        Results = [.. results];
    }

    public IInvokableTask Task { get; }
    public Collection<ValidateResult> Results { get; }
}