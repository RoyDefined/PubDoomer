using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Engine.Saving;
using PubDoomer.Project.Tasks;
using System.Collections.ObjectModel;

namespace PubDoomer.Tasks.FileSystem.MoveFolder;

public partial class ObservableMoveFolderTask : ProjectTaskBase
{

    public const string TaskName = "Move folder";
    private const string TaskDescription = "Moves the given source folder to the given target folder. Optionally the task recursively moves all sub folders.";

    public override Type HandlerType => typeof(MoveFolderTaskHandler);
    public override string DisplayName => TaskName;
    public override string Description => TaskDescription;

    /// <summary>
    /// The source folder to move.
    /// </summary>
    [ObservableProperty] private string? _sourceFolder;
    
    /// <summary>
    /// The target folder to move the folder to.
    /// </summary>
    [ObservableProperty] private string? _targetFolder;
    
    /// <summary>
    /// If <see langword="true"/>, recursively copy the contents of sub folders as well.
    /// </summary>
    [ObservableProperty] private bool _recursive;

    public ObservableMoveFolderTask()
    {
    }

    public ObservableMoveFolderTask(
        string? name, string? sourceFolder = null, string? targetFolder = null, bool recursive = true)
    {
        Name = name;
        SourceFolder = sourceFolder;
        TargetFolder = targetFolder;
        Recursive = recursive;
    }


    public override ObservableMoveFolderTask DeepClone()
    {
        return new ObservableMoveFolderTask(Name, SourceFolder, TargetFolder, Recursive);
    }

    public override void Merge(ProjectTaskBase task)
    {
        if (task is not ObservableMoveFolderTask moveFolderTask)
        {
            // TODO: Error?
            return;
        }

        Name = moveFolderTask.Name;
        SourceFolder = moveFolderTask.SourceFolder;
        TargetFolder = moveFolderTask.TargetFolder;
        Recursive = moveFolderTask.Recursive;
    }

    public override void Serialize(IProjectWriter writer)
    {
        writer.Write(SourceFolder);
        writer.Write(TargetFolder);
        writer.Write(Recursive);
    }

    public override void Deserialize(IProjectReader reader, ProjectSaveVersion version)
    {
        SourceFolder = reader.ReadString();
        TargetFolder = reader.ReadString();
        Recursive = reader.ReadBoolean();
    }
}