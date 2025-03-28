using System.Diagnostics;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Project.Tasks;

namespace PubDoomer.Tasks.Compile.Acc;

public partial class ObservableAccCompileTask : CompileTaskBase
{
    private const string TaskName = "Compile (ACC)";
    private const string TaskDescription = "Compiles the ACS file from the given file path using an ACS compiler.";

    [JsonIgnore] public override Type HandlerType => typeof(AccCompileTaskHandler);
    [JsonIgnore] public override CompilerType Type => CompilerType.Acc;
    [JsonIgnore] public override string[] ExpectedFileExtensions { get; } = [".acs", ".txt"];

    [ObservableProperty] private bool _keepAccErrFile;

    // TODO: Implement additional parameters
    // [ObservableProperty] private string? _includeFolder;
    // [ObservableProperty] private bool _outputDebug;
    // [ObservableProperty] private string? _debugFilePath;
    // [ObservableProperty] private AccBytecodeLevel _bytecodeLevel;

    public ObservableAccCompileTask()
    {
    }

    public ObservableAccCompileTask(string? name, string? inputFilePath, string? outputFilePath, bool keepAccErrFile = false)
        : base(name, inputFilePath, outputFilePath)
    {
        KeepAccErrFile = keepAccErrFile;
    }

    [JsonIgnore] public override string DisplayName => TaskName;
    [JsonIgnore] public override string Description => TaskDescription;

    public override ObservableAccCompileTask DeepClone()
    {
        return new ObservableAccCompileTask(Name, InputFilePath, OutputFilePath, (bool)this.KeepAccErrFile);
    }

    public override void Merge(ProjectTaskBase task)
    {
        if (task is not ObservableAccCompileTask accCompileTask)
        {
            // TODO: Error?
            return;
        }

        Name = accCompileTask.Name;
        InputFilePath = accCompileTask.InputFilePath;
        OutputFilePath = accCompileTask.OutputFilePath;
        GenerateStdOutAndStdErrFiles = accCompileTask.GenerateStdOutAndStdErrFiles;
        KeepAccErrFile = accCompileTask.KeepAccErrFile;
    }
}