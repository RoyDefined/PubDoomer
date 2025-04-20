using System;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Engine.Saving;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;

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
    public abstract void Serialize(IProjectWriter writer);

    /// <summary>
    /// Deserializes the task into the given reader.
    /// </summary>
    public abstract void Deserialize(IProjectReader reader, ProjectSaveVersion version);
}