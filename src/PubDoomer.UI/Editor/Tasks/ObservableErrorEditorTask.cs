using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json.Serialization;
using Avalonia.Controls;
using PubDoomer.Engine.Saving;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using PubDoomer.Engine.TaskInvokation.Validation;
using PubDoomer.Project.Tasks;

namespace PubDoomer.UI.Editor.Tasks;

// Represents a task that will always return two errors.
public partial class ObservableErrorEditorTask : ProjectTaskBase, ITaskValidator
{
    private static readonly Type HandlerTypeCached = typeof(ErrorEditorTaskHandler);

    private const string TaskName = "Error Editor Task";
    private const string TaskDescription = "A task that will always return two errors.";
    public override Type HandlerType => HandlerTypeCached;

    public ObservableErrorEditorTask()
    {
        if (!Design.IsDesignMode) throw new InvalidOperationException("This task is not designed to be executed outside of the designer.");
    }

    public ObservableErrorEditorTask(string? name)
    {
        if (!Design.IsDesignMode) throw new InvalidOperationException("This task is not designed to be executed outside of the designer.");
        
        Name = name;
    }

    [JsonIgnore] public override string DisplayName => TaskName;
    [JsonIgnore] public override string Description => TaskDescription;

    public override ObservableErrorEditorTask DeepClone()
    {
        throw new NotImplementedException();
    }

    public override void Merge(ProjectTaskBase task)
    {
        throw new NotImplementedException();
    }

    public override void Serialize(IProjectWriter writer)
    {
        throw new NotImplementedException();
    }

    public override void Deserialize(IProjectReader reader, ProjectSaveVersion _)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<ValidateResult> Validate()
    {
        yield return ValidateResult.FromError("Error number #1");
        yield return ValidateResult.FromError("Error number #2");
    }
}