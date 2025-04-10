using PubDoomer.Engine.Saving;
using PubDoomer.Project.Tasks;

namespace PubDoomer.Tasks.CopyProject;

public partial class ObservableCopyProjectTask : ProjectTaskBase
{

    public const string TaskName = "Copy project";
    private const string TaskDescription = "Copies the project as a whole to a new location and updates the working directory to it.";

    public override Type HandlerType => typeof(CopyProjectTaskHandler);
    public override string DisplayName => TaskName;
    public override string Description => TaskDescription;

    public override ProjectTaskBase DeepClone()
    {
        throw new NotImplementedException();
    }

    public override void Deserialize(IProjectReader reader, ProjectSaveVersion version)
    {
        throw new NotImplementedException();
    }

    public override void Merge(ProjectTaskBase task)
    {
        throw new NotImplementedException();
    }

    public override void Serialize(IProjectWriter writer)
    {
        throw new NotImplementedException();
    }
}