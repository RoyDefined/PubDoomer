using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Engine.Saving;
using PubDoomer.Project.Tasks;
using System.Collections.ObjectModel;

namespace PubDoomer.Tasks.FileSystem.CopyProject;

public partial class ObservableCopyProjectTask : ProjectTaskBase
{

    public const string TaskName = "Copy project";
    private const string TaskDescription = "Copies the project as a whole to a new location and updates the working directory to it.";

    public override Type HandlerType => typeof(CopyProjectTaskHandler);
    public override string DisplayName => TaskName;
    public override string Description => TaskDescription;

    /// <summary>
    /// The target folder to copy the project to.
    /// </summary>
    [ObservableProperty] private string? _targetFolder;
    
    /// <summary>
    /// If <see langword="true"/>, copy the project over to a temporary directory generated during invokation.
    /// </summary>
    [ObservableProperty] private bool _useTempFolder = true;

    public ObservableCopyProjectTask()
    {
    }

    public ObservableCopyProjectTask(
        string? name, string? targetFolder = null, bool useTempFolder = true)
    {
        Name = name;
        TargetFolder = targetFolder;
        UseTempFolder = useTempFolder;
    }


    public override ObservableCopyProjectTask DeepClone()
    {
        return new ObservableCopyProjectTask(Name, TargetFolder, UseTempFolder);
    }

    public override void Merge(ProjectTaskBase task)
    {
        if (task is not ObservableCopyProjectTask copyProjectTask)
        {
            // TODO: Error?
            return;
        }

        Name = copyProjectTask.Name;
        TargetFolder = copyProjectTask.TargetFolder;
        UseTempFolder = copyProjectTask.UseTempFolder;
    }

    public override void Serialize(IProjectWriter writer)
    {
        writer.Write(TargetFolder);
        writer.Write(UseTempFolder);
    }

    public override void Deserialize(IProjectReader reader, ProjectSaveVersion version)
    {
        TargetFolder = reader.ReadString();
        UseTempFolder = reader.ReadBoolean();
    }
}