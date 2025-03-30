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

// Represents a task that will always return two warnings.
public partial class ObservableWarningEditorTask : ProjectTaskBase, ITaskValidator
{
    private static readonly Type HandlerTypeCached = typeof(WarningEditorTaskHandler);

    private const string TaskName = "Warning Editor Task";
    private const string TaskDescription = "A task that will always return two warning.";

    public override Type HandlerType => HandlerTypeCached;

    public ObservableWarningEditorTask()
    {
        if (!Design.IsDesignMode) throw new InvalidOperationException("This task is not designed to be executed outside of the designer.");
    }

    public ObservableWarningEditorTask(string? name)
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
        yield return ValidateResult.FromWarning("Warning number #1");
        yield return ValidateResult.FromWarning("Warning number #2");
    }
}