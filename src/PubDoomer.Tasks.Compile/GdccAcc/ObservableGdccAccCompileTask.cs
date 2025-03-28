using System.Diagnostics;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Project.Tasks;
using static CommunityToolkit.Mvvm.ComponentModel.__Internals.__TaskExtensions.TaskAwaitableWithoutEndValidation;

namespace PubDoomer.Tasks.Compile.GdccAcc;

public partial class ObservableGdccAccCompileTask : CompileTaskBase
{
    public const string TaskName = "Compile (GDCC-ACC)";
    private const string TaskDescription = "Compiles the ACS file from the given file path using a GDCC-ACC compiler.";

    [JsonIgnore] public override Type HandlerType => typeof(GdccAccCompileTaskHandler);
    [JsonIgnore] public override CompilerType Type => CompilerType.GdccAcc;
    [JsonIgnore] public override string[] ExpectedFileExtensions { get; } = [".acs", ".txt"];


    [ObservableProperty] private bool _dontWarnForwardReferences;

    // TODO: Implement additional parameters assuming these exist?

    public ObservableGdccAccCompileTask()
    {
    }

    public ObservableGdccAccCompileTask(string? name, string? inputFilePath, string? outputFilePath, bool dontWarnForwardReferences = false)
        : base(name, inputFilePath, outputFilePath)
    {
        DontWarnForwardReferences = dontWarnForwardReferences;
    }

    [JsonIgnore] public override string DisplayName => TaskName;
    [JsonIgnore] public override string Description => TaskDescription;

    public override ObservableGdccAccCompileTask DeepClone()
    {
        return new ObservableGdccAccCompileTask(Name, InputFilePath, OutputFilePath, (bool)this.DontWarnForwardReferences);
    }

    public override void Merge(ProjectTaskBase task)
    {
        if (task is not ObservableGdccAccCompileTask gdccAccCompileTask)
        {
            // TODO: Error?
            return;
        }

        Name = gdccAccCompileTask.Name;
        InputFilePath = gdccAccCompileTask.InputFilePath;
        OutputFilePath = gdccAccCompileTask.OutputFilePath;
        GenerateStdOutAndStdErrFiles = gdccAccCompileTask.GenerateStdOutAndStdErrFiles;
        DontWarnForwardReferences = gdccAccCompileTask.DontWarnForwardReferences;
    }

    public override void Serialize(BinaryWriter writer)
    {
        base.Serialize(writer);
        writer.Write(DontWarnForwardReferences);
    }

    public override void Deserialize(BinaryReader reader)
    {
        base.Deserialize(reader);
        DontWarnForwardReferences = reader.ReadBoolean();
    }
}