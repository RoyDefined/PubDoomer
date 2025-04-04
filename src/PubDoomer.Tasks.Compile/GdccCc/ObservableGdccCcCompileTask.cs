using System.Diagnostics;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Engine.Saving;
using PubDoomer.Project.Tasks;
using PubDoomer.Tasks.Compile.GdccCc;

namespace PubDoomer.Tasks.Compile.GdccCc;

public partial class ObservableGdccCcCompileTask : CompileTaskBase
{
    public const string TaskName = "Compile (GDCC-CC)";
    private const string TaskDescription = "Compiles the C file from the given file path using a GDCC-CC compiler.";

    public override Type HandlerType => typeof(GdccCcCompileTaskHandler);
    public override CompilerType Type => CompilerType.GdccCc;
    public override string[] ExpectedFileExtensions { get; } = [".c", ".txt"];

    [ObservableProperty] private TargetEngineType _targetEngine;
    
    /// <summary>
    /// If <c>true</c>, build and link libc with the compiled file.
    /// </summary>
    [ObservableProperty] private bool _linkLibc;
    
    /// <summary>
    /// If <c>true</c>, build and link libGDCC with the compiled file.
    /// </summary>
    [ObservableProperty] private bool _linkLibGdcc;

    // TODO: Implement additional parameters.

    public ObservableGdccCcCompileTask()
    {
    }

    public ObservableGdccCcCompileTask(
        string? name, string? inputFilePath, string? outputFilePath, TargetEngineType targetEngine = TargetEngineType.None,
        bool linkLibc = true, bool linkLibGdcc = true)
        : base(name, inputFilePath, outputFilePath)
    {
        TargetEngine = targetEngine;
        LinkLibc = linkLibc;
        LinkLibGdcc = linkLibGdcc;
    }

    public override string DisplayName => TaskName;
    public override string Description => TaskDescription;

    public override ObservableGdccCcCompileTask DeepClone()
    {
        return new ObservableGdccCcCompileTask(
            Name, InputFilePath, OutputFilePath, TargetEngine,
            LinkLibc, LinkLibGdcc);
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
        LinkLibc = gdccCcCompileTask.LinkLibc;
        LinkLibGdcc = gdccCcCompileTask.LinkLibGdcc;
    }

    public override void Serialize(IProjectWriter writer)
    {
        base.Serialize(writer);
        writer.WriteEnum<TargetEngineType>(TargetEngine);
        writer.Write(LinkLibc);
        writer.Write(LinkLibGdcc);
    }

    public override void Deserialize(IProjectReader reader, ProjectSaveVersion version)
    {
        base.Deserialize(reader, version);
        TargetEngine = reader.ReadEnum<TargetEngineType>();
        LinkLibc = reader.ReadBoolean();
        LinkLibGdcc = reader.ReadBoolean();
    }
}