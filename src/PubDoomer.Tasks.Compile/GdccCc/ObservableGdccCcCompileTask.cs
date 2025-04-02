using System.Diagnostics;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Engine.Compile;
using PubDoomer.Engine.Saving;
using PubDoomer.Project.Tasks;

namespace PubDoomer.Tasks.Compile.GdccAcc;

public partial class ObservableGdccCcCompileTask : CompileTaskBase
{
    public const string TaskName = "Compile (GDCC-CC)";
    private const string TaskDescription = "Compiles the C file from the given file path using a GDCC-CC compiler.";

    public override Type HandlerType => typeof(GdccCcCompileTaskHandler);
    public override CompilerType Type => CompilerType.GdccCc;
    public override string[] ExpectedFileExtensions { get; } = [".c", ".txt"];

    [ObservableProperty] private TargetEngineType _targetEngine;
    [ObservableProperty] private bool _dontBuildLibGdcc;

    // TODO: Implement additional parameters.

    public ObservableGdccCcCompileTask()
    {
    }

    public ObservableGdccCcCompileTask(string? name, string? inputFilePath, string? outputFilePath, TargetEngineType targetEngine = TargetEngineType.None, bool dontBuildLibGdcc = false)
        : base(name, inputFilePath, outputFilePath)
    {
        TargetEngine = targetEngine;
        _dontBuildLibGdcc = dontBuildLibGdcc;
    }

    public override string DisplayName => TaskName;
    public override string Description => TaskDescription;

    public override ObservableGdccCcCompileTask DeepClone()
    {
        return new ObservableGdccCcCompileTask(Name, InputFilePath, OutputFilePath, TargetEngine, DontBuildLibGdcc);
    }

    public override void Merge(ProjectTaskBase task)
    {
        if (task is not ObservableGdccCcCompileTask gdccCcCompileTask)
        {
            // TODO: Error?
            return;
        }

        Name = gdccCcCompileTask.Name;
        InputFilePath = gdccCcCompileTask.InputFilePath;
        OutputFilePath = gdccCcCompileTask.OutputFilePath;
        GenerateStdOutAndStdErrFiles = gdccCcCompileTask.GenerateStdOutAndStdErrFiles;
        TargetEngine = gdccCcCompileTask.TargetEngine;
        DontBuildLibGdcc = gdccCcCompileTask.DontBuildLibGdcc;
    }

    public override void Serialize(IProjectWriter writer)
    {
        base.Serialize(writer);
        writer.WriteEnum<TargetEngineType>(TargetEngine);
        writer.Write(DontBuildLibGdcc);
    }

    public override void Deserialize(IProjectReader reader, ProjectSaveVersion version)
    {
        base.Deserialize(reader, version);
        TargetEngine = reader.ReadEnum<TargetEngineType>();
        DontBuildLibGdcc = reader.ReadBoolean();
    }
}