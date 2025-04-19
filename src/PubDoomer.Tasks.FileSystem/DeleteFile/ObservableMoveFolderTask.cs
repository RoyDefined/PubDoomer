using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Engine.Saving;
using PubDoomer.Project.Tasks;
using System.Collections.ObjectModel;

namespace PubDoomer.Tasks.FileSystem.DeleteFile;

public partial class ObservableDeleteFileTask : ProjectTaskBase
{

    public const string TaskName = "Delete file";
    private const string TaskDescription = "Deletes the given target file.";

    public override Type HandlerType => typeof(DeleteFileTaskHandler);
    public override string DisplayName => TaskName;
    public override string Description => TaskDescription;

    /// <summary>
    /// The target file path to delete.
    /// </summary>
    [ObservableProperty] private string? _targetFilePath;

    public ObservableDeleteFileTask()
    {
    }

    public ObservableDeleteFileTask(
        string? name, string? targetFilePath = null)
    {
        Name = name;
        TargetFilePath = targetFilePath;
    }


    public override ObservableDeleteFileTask DeepClone()
    {
        return new ObservableDeleteFileTask(Name, TargetFilePath);
    }

    public override void Merge(ProjectTaskBase task)
    {
        if (task is not ObservableDeleteFileTask deletefileTask)
        {
            // TODO: Error?
            return;
        }

        Name = deletefileTask.Name;
        TargetFilePath = deletefileTask.TargetFilePath;
    }

    public override void Serialize(IProjectWriter writer)
    {
        writer.Write(TargetFilePath);
    }

    public override void Deserialize(IProjectReader reader, ProjectSaveVersion version)
    {
        TargetFilePath = reader.ReadString();
    }
}