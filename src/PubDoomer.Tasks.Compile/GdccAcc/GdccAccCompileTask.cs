using System.Diagnostics;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Project.Tasks;

namespace PubDoomer.Tasks.Compile.GdccAcc;

public partial class GdccAccCompileTask : CompileTaskBase
{
    public const string TaskName = "Compile (GDCC-ACC)";
    private const string TaskDescription = "Compiles the ACS file from the given file path using a GDCC-ACC compiler.";

    private static readonly Type HandlerTypeCached = typeof(EngineGdccAccCompileTaskHandler);
    public override Type HandlerType => HandlerTypeCached;
    protected override string[] ExpectedFileExtensions { get; } = [".acs", ".txt"];


    [ObservableProperty] private bool _dontWarnForwardReferences;

    // TODO: Implement additional parameters assuming these exist?

    public GdccAccCompileTask()
    {
    }

    public GdccAccCompileTask(string? name, string? inputFilePath, string? outputFilePath, bool dontWarnForwardReferences = false)
        : base(name, inputFilePath, outputFilePath)
    {
        DontWarnForwardReferences = dontWarnForwardReferences;
    }

    public override CompilerType Type => CompilerType.GdccAcc;

    [JsonIgnore] public override string DisplayName => TaskName;
    [JsonIgnore] public override string Description => TaskDescription;

    public override GdccAccCompileTask ToEngineTaskBase()
    {
        Debug.Assert(InputFilePath != null && OutputFilePath != null && Name != null);

        return new GdccAccCompileTask
        {
            Name = Name,
            InputFilePath = InputFilePath,
            OutputFilePath = OutputFilePath,
            GenerateStdOutAndStdErrFiles = GenerateStdOutAndStdErrFiles,
            DontWarnForwardReferences = DontWarnForwardReferences
        };
    }

    public override GdccAccCompileTask DeepClone()
    {
        return new GdccAccCompileTask(Name, InputFilePath, OutputFilePath, DontWarnForwardReferences);
    }


    public override void Merge(ProjectTaskBase task)
    {
        if (task is not GdccAccCompileTask gdccAccCompileTask)
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
}