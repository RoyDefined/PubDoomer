using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Engine.Saving;
using PubDoomer.Project.Tasks;
using System.Collections.ObjectModel;

namespace PubDoomer.Tasks.FileSystem.ZipFolder;

public partial class ObservableZipFolderTask : ProjectTaskBase
{

    public const string TaskName = "Zip folder";
    private const string TaskDescription = "Zips the given source folder to the given target folder.";

    public override Type HandlerType => typeof(ZipFolderTaskHandler);
    public override string DisplayName => TaskName;
    public override string Description => TaskDescription;

    /// <summary>
    /// The source folder to zip.
    /// </summary>
    [ObservableProperty] private string? _sourceFolder;
    
    /// <summary>
    /// The target file path to zip the folder to.
    /// </summary>
    [ObservableProperty] private string? _targetFilePath;

    public ObservableZipFolderTask()
    {
    }

    public ObservableZipFolderTask(
        string? name, string? sourceFolder = null, string? targetFilePath = null)
    {
        Name = name;
        SourceFolder = sourceFolder;
        TargetFilePath = targetFilePath;
    }


    public override ObservableZipFolderTask DeepClone()
    {
        return new ObservableZipFolderTask(Name, SourceFolder, TargetFilePath);
    }

    public override void Merge(ProjectTaskBase task)
    {
        if (task is not ObservableZipFolderTask copyFolderTask)
        {
            // TODO: Error?
            return;
        }

        Name = copyFolderTask.Name;
        SourceFolder = copyFolderTask.SourceFolder;
        TargetFilePath = copyFolderTask.TargetFilePath;
    }

    public override void Serialize(IProjectWriter writer)
    {
        writer.Write(SourceFolder);
        writer.Write(TargetFilePath);
    }

    public override void Deserialize(IProjectReader reader, ProjectSaveVersion version)
    {
        SourceFolder = reader.ReadString();
        TargetFilePath = reader.ReadString();
    }
}