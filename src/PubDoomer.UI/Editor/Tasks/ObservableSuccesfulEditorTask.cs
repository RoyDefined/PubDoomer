using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json.Serialization;
using Avalonia.Controls;
using PubDoomer.Engine.Saving;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using PubDoomer.Project.Tasks;

namespace PubDoomer.UI.Editor.Tasks;

// Represents a task that will always complete succesfully.
public partial class ObservableSuccesfulEditorTask : ProjectTaskBase
{
    private static readonly Type HandlerTypeCached = typeof(SuccesfulEditorTaskHandler);

    private const string TaskName = "Succesful Editor Task";
    private const string TaskDescription = "A task that will always complete.";

    public override Type HandlerType => HandlerTypeCached;

    public ObservableSuccesfulEditorTask()
    {
        if (!Design.IsDesignMode) throw new InvalidOperationException("This task is not designed to be executed outside of the designer.");
    }

    public ObservableSuccesfulEditorTask(string? name)
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

    public override void Deserialize(IProjectReader reader)
    {
        throw new NotImplementedException();
    }
}