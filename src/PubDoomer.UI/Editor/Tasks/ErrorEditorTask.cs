using System;
using System.Diagnostics;
using System.Text.Json.Serialization;
using Avalonia.Controls;
using PubDoomer.Project.Tasks;

namespace PubDoomer.UI.Editor.Tasks;

// Represents a task that will always return two errors.
public partial class ErrorEditorTask : ProjectTaskBase
{
    private const string TaskName = "Error Editor Task";
    private const string TaskDescription = "A task that will always return two errors.";

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

    public override ErrorEditorEngineTask ToEngineTaskBase()
    {
        Debug.Assert(Name != null);
        return new ErrorEditorEngineTask
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