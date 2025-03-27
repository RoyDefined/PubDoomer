using System.Diagnostics;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Tasks.Compile;
using PubDoomer.Tasks.Compile.GdccAcc;

namespace PubDoomer.Project.Tasks;

public partial class GdccAccCompileTask : CompileTaskBase
{
    public const string TaskName = "Compile (GDCC-ACC)";
    private const string TaskDescription = "Compiles the ACS file from the given file path using a GDCC-ACC compiler.";

    
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

    public override EngineGdccAccCompileTask ToEngineTaskBase()
    {
        Debug.Assert(InputFilePath != null && OutputFilePath != null && Name != null);
        
        return new EngineGdccAccCompileTask
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