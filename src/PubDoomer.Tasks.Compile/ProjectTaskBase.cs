using System;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using PubDoomer.Tasks.Compile.Acc;
using PubDoomer.Tasks.Compile.Bcc;
using PubDoomer.Tasks.Compile.GdccAcc;

namespace PubDoomer.Project.Tasks;

// TODO: Convert into interface.
public abstract partial class ProjectTaskBase : ObservableObject, IRunnableTask, ICloneable
{
    [ObservableProperty] private string? _name;
    public abstract string DisplayName { get; }
    public abstract string Description { get; }

    public abstract Type HandlerType { get; }
    public virtual Type? ValidatorType { get; }

    public object Clone()
    {
        return DeepClone();
    }

    public abstract ProjectTaskBase DeepClone();
    public abstract void Merge(ProjectTaskBase task);

    /// <summary>
    /// Serializes the task into the given writer.
    /// </summary>
    public abstract void Serialize(BinaryWriter writer);

    /// <summary>
    /// Deserializes the task into the given reader.
    /// </summary>
    public abstract void Deserialize(BinaryReader reader);
}