using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Engine.Saving;
using PubDoomer.Project.Tasks;
using System.Collections.ObjectModel;

namespace PubDoomer.Tasks.FileSystem.MoveFile;

public partial class ObservableMoveFileTask : ProjectTaskBase
{

    public const string TaskName = "Move file";
    private const string TaskDescription = "Moves the given source file to the given target file.";

    public override Type HandlerType => typeof(MoveFileTaskHandler);
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

    public ObservableMoveFileTask()
    {
    }

    public ObservableMoveFileTask(
        string? name, string? sourceFile = null, string? targetFile = null)
    {
        Name = name;
        SourceFile = sourceFile;
        TargetFile = targetFile;
    }


    public override ObservableMoveFileTask DeepClone()
    {
        return new ObservableMoveFileTask(Name, SourceFile, TargetFile);
    }

    public override void Merge(ProjectTaskBase task)
    {
        if (task is not ObservableMoveFileTask moveFileTask)
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