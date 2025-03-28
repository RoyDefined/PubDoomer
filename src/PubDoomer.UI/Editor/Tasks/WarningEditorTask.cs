using System;
using System.Diagnostics;
using System.Text.Json.Serialization;
using Avalonia.Controls;
using PubDoomer.Project.Tasks;

namespace PubDoomer.UI.Editor.Tasks;

// Represents a task that will always return two warnings.
public partial class WarningEditorTask : ProjectTaskBase
{
    private const string TaskName = "Warning Editor Task";
    private const string TaskDescription = "A task that will always return two warning.";

    public WarningEditorTask()
    {
        if (!Design.IsDesignMode) throw new InvalidOperationException("This task is not designed to be executed outside of the designer.");
    }

    public WarningEditorTask(string? name)
    {
        if (!Design.IsDesignMode) throw new InvalidOperationException("This task is not designed to be executed outside of the designer.");
        
        Name = name;
    }

    [JsonIgnore] public override string DisplayName => TaskName;
    [JsonIgnore] public override string Description => TaskDescription;

    public override WarningEditorEngineTask ToEngineTaskBase()
    {
        Debug.Assert(Name != null);
        return new WarningEditorEngineTask
        {
            Name = Name,
        };
    }

    public override ErrorEditorTask DeepClone()
    {
        throw new NotImplementedException();
    }

    public override void Merge(ProjectTaskBase task)
    {
        throw new NotImplementedException();
    }
}