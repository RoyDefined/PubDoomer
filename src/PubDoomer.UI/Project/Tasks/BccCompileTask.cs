using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Engine.Compile.Bcc;

namespace PubDoomer.Project.Tasks;

public partial class BccCompileTask : CompileTaskBase
{
    public const string TaskName = "Compile (BCC)";
    private const string TaskDescription = "Compiles the ACS file from the given file path using a BCC compiler.";

    // TODO: Implement additional parameters
    // [ObservableProperty] private bool _accErrorFile;
    // [ObservableProperty] private bool _accStats;
    // [ObservableProperty] private bool _help; // I don't think this one really makes sense.
    // [ObservableProperty] private Collection<string> _fileDirectories;
    // [ObservableProperty] private bool _oneColumn;
    // [ObservableProperty] private int? _tabSize;
    // [ObservableProperty] private bool _stripAsserts;
    // [ObservableProperty] private bool _preprocessOnly;
    // [ObservableProperty] private Collection<string> _macros;
    // [ObservableProperty] private Collection<string> _linkLibraries;
    
    public BccCompileTask()
    {
    }

    public BccCompileTask(string? name, string? inputFilePath, string? outputFilePath)
        : base(name, inputFilePath, outputFilePath)
    {
    }

    [JsonIgnore] public override string DisplayName => TaskName;
    [JsonIgnore] public override string Description => TaskDescription;

    public override EngineBccCompileTask ToEngineTaskBase()
    {
        Debug.Assert(InputFilePath != null && OutputFilePath != null && Name != null);
        
        return new EngineBccCompileTask
        {
            Name = Name,
            InputFilePath = InputFilePath,
            OutputFilePath = OutputFilePath,
            GenerateStdOutAndStdErrFiles = GenerateStdOutAndStdErrFiles,
        };
    }

    public override BccCompileTask DeepClone()
    {
        return new BccCompileTask(Name, InputFilePath, OutputFilePath);
    }

    public override void Merge(ProjectTaskBase task)
    {
        if (task is not BccCompileTask bccCompileTask)
        {
            // TODO: Error?
            return;
        }

        Name = bccCompileTask.Name;
        InputFilePath = bccCompileTask.InputFilePath;
        OutputFilePath = bccCompileTask.OutputFilePath;
        GenerateStdOutAndStdErrFiles = bccCompileTask.GenerateStdOutAndStdErrFiles;
    }
}