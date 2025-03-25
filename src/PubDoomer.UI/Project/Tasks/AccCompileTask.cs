using System.Diagnostics;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Engine.Compile;
using PubDoomer.Engine.Compile.Acc;

namespace PubDoomer.Project.Tasks;

public partial class AccCompileTask : CompileTaskBase
{
    private const string TaskName = "Compile (ACC)";
    private const string TaskDescription = "Compiles the ACS file from the given file path using an ACS compiler.";
    
    [ObservableProperty] private bool _keepAccErrFile;
    
    // TODO: Implement additional parameters
    // [ObservableProperty] private string? _includeFolder;
    // [ObservableProperty] private bool _outputDebug;
    // [ObservableProperty] private string? _debugFilePath;
    // [ObservableProperty] private AccBytecodeLevel _bytecodeLevel;

    public AccCompileTask()
    {
    }

    public AccCompileTask(string? name, string? inputFilePath, string? outputFilePath, bool keepAccErrFile = false)
        : base(name, inputFilePath, outputFilePath)
    {
        KeepAccErrFile = keepAccErrFile;
    }

    [JsonIgnore] public override string DisplayName => TaskName;
    [JsonIgnore] public override string Description => TaskDescription;

    public override EngineAccCompileTask ToEngineTaskBase()
    {
        Debug.Assert(InputFilePath != null && OutputFilePath != null && Name != null);
        
        return new EngineAccCompileTask
        {
            Name = Name,
            InputFilePath = InputFilePath,
            OutputFilePath = OutputFilePath,
            GenerateStdOutAndStdErrFiles = GenerateStdOutAndStdErrFiles,
            KeepAccErrFile = KeepAccErrFile,
        };
    }

    public override CompilerType Type => CompilerType.Acc;

    public override AccCompileTask DeepClone()
    {
        return new AccCompileTask(Name, InputFilePath, OutputFilePath, KeepAccErrFile);
    }

    public override void Merge(ProjectTaskBase task)
    {
        if (task is not AccCompileTask accCompileTask)
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