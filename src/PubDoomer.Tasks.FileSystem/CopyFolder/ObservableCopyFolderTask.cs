using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Engine.Saving;
using PubDoomer.Project.Tasks;
using System.Collections.ObjectModel;

namespace PubDoomer.Tasks.FileSystem.CopyFolder;

public partial class ObservableCopyFolderTask : ProjectTaskBase
{

    public const string TaskName = "Copy folder";
    private const string TaskDescription = "Copies the given source folder to the given target folder. Optionally the task recursively copies all sub folders.";

    public override Type HandlerType => typeof(CopyFolderTaskHandler);
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

    public ObservableCopyFolderTask()
    {
    }

    public ObservableCopyFolderTask(
        string? name, string? sourceFolder = null, string? targetFolder = null, bool recursive = true)
    {
        Name = name;
        SourceFolder = sourceFolder;
        TargetFolder = targetFolder;
        Recursive = recursive;
    }


    public override ObservableCopyFolderTask DeepClone()
    {
        return new ObservableCopyFolderTask(Name, SourceFolder, TargetFolder, Recursive);
    }

    public override void Merge(ProjectTaskBase task)
    {
        if (task is not ObservableCopyFolderTask copyFolderTask)
        {
            // TODO: Error?
            return;
        }

        Name = copyFolderTask.Name;
        SourceFolder = copyFolderTask.SourceFolder;
        TargetFolder = copyFolderTask.TargetFolder;
        Recursive = copyFolderTask.Recursive;
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