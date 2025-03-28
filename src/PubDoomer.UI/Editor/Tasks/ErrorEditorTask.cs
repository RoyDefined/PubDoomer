using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;
using Avalonia.Controls;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using PubDoomer.Engine.TaskInvokation.Validation;
using PubDoomer.Project.Tasks;

namespace PubDoomer.UI.Editor.Tasks;

// Represents a task that will always return two errors.
public partial class ErrorEditorTask : ProjectTaskBase, IValidatableTask
{
    private static readonly Type HandlerTypeCached = typeof(ErrorEditorEngineTaskHandler);

    private const string TaskName = "Error Editor Task";
    private const string TaskDescription = "A task that will always return two errors.";
    public override Type HandlerType => HandlerTypeCached;

    public ErrorEditorTask()
    {
        if (!Design.IsDesignMode) throw new InvalidOperationException("This task is not designed to be executed outside of the designer.");
    }

    public ErrorEditorTask(string? name)
    {
        if (!Design.IsDesignMode) throw new InvalidOperationException("This task is not designed to be executed outside of the designer.");
        
        Name = name;
    }

    [JsonIgnore] public override string DisplayName => TaskName;
    [JsonIgnore] public override string Description => TaskDescription;

    public override ErrorEditorTask DeepClone()
    {
        throw new NotImplementedException();
    }

    public override void Merge(ProjectTaskBase task)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<ValidateResult> Validate()
    {
        yield return ValidateResult.FromError("Error number #1");
        yield return ValidateResult.FromError("Error number #2");
    }
}