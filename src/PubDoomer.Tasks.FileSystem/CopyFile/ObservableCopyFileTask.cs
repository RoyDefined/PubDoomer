using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Engine.Saving;
using PubDoomer.Project.Tasks;
using System.Collections.ObjectModel;

namespace PubDoomer.Tasks.FileSystem.CopyFile;

public partial class ObservableCopyFileTask : ProjectTaskBase
{
    
    public const string TaskName = "Copy file";
    private const string TaskDescription = "Copies the given source file to the given target file.";

    public override Type HandlerType => typeof(CopyFileTaskHandler);
    public override string DisplayName => TaskName;
    public override string Description => TaskDescription;

    /// <summary>
    /// The source file to move.
    /// </summary>
    [ObservableProperty] private string? _sourceFile;
    
    /// <summary>
    /// The target file to move the file to.
    /// </summary>
    [ObservableProperty] private string? _targetFile;

    public ObservableCopyFileTask()
    {
    }

    public ObservableCopyFileTask(
        string? name, string? sourceFile = null, string? targetFile = null)
    {
        Name = name;
        SourceFile = sourceFile;
        TargetFile = targetFile;
    }


    public override ObservableCopyFileTask DeepClone()
    {
        return new ObservableCopyFileTask(Name, SourceFile, TargetFile);
    }

    public override void Merge(ProjectTaskBase task)
    {
        if (task is not ObservableCopyFileTask moveFileTask)
        {
            // TODO: Error?
            return;
        }

        Name = moveFileTask.Name;
        SourceFile = moveFileTask.SourceFile;
        TargetFile = moveFileTask.TargetFile;
    }

    public override void Serialize(IProjectWriter writer)
    {
        writer.Write(SourceFile);
        writer.Write(TargetFile);
    }

    public override void Deserialize(IProjectReader reader, ProjectSaveVersion version)
    {
        SourceFile = reader.ReadString();
        TargetFile = reader.ReadString();
    }
}