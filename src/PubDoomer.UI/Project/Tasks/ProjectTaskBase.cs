using System;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Engine.TaskInvokation.Task;

namespace PubDoomer.Project.Tasks;

[JsonDerivedType(typeof(AccCompileTask), "AccCompile")]
[JsonDerivedType(typeof(BccCompileTask), "BccCompile")]
[JsonDerivedType(typeof(GdccAccCompileTask), "GdccAccCompile")]
public abstract partial class ProjectTaskBase : ObservableObject, ICloneable
{
    [ObservableProperty] private string? _name;
    public abstract string DisplayName { get; }
    public abstract string Description { get; }

    public object Clone()
    {
        return DeepClone();
    }

    public abstract EngineTaskBase ToEngineTaskBase();
    public abstract ProjectTaskBase DeepClone();
    public abstract void Merge(ProjectTaskBase task);
}