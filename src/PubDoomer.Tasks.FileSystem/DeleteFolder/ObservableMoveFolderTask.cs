using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Engine.Saving;
using PubDoomer.Project.Tasks;
using System.Collections.ObjectModel;

namespace PubDoomer.Tasks.FileSystem.DeleteFolder;

public partial class ObservableDeleteFolderTask : ProjectTaskBase
{

    public const string TaskName = "Delete folder";
    private const string TaskDescription = "Deletes the given target folder.";

    public override Type HandlerType => typeof(DeleteFolderTaskHandler);
    public override string DisplayName => TaskName;
    public override string Description => TaskDescription;

    /// <summary>
    /// The target folder to delete.
    /// </summary>
    [ObservableProperty] private string? _targetFolder;

    public ObservableDeleteFolderTask()
    {
    }

    public ObservableDeleteFolderTask(
        string? name, string? targetFolder = null)
    {
        Name = name;
        TargetFolder = targetFolder;
    }


    public override ObservableDeleteFolderTask DeepClone()
    {
        return new ObservableDeleteFolderTask(Name, TargetFolder);
    }

    public override void Merge(ProjectTaskBase task)
    {
        if (task is not ObservableDeleteFolderTask deleteFolderTask)
        {
            // TODO: Error?
            return;
        }

        Name = deleteFolderTask.Name;
        TargetFolder = deleteFolderTask.TargetFolder;
    }

    public override void Serialize(IProjectWriter writer)
    {
        writer.Write(TargetFolder);
    }

    public override void Deserialize(IProjectReader reader, ProjectSaveVersion version)
    {
        TargetFolder = reader.ReadString();
    }
}