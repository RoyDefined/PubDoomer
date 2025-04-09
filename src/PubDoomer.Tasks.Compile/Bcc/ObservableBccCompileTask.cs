using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Engine.Saving;
using PubDoomer.Project.Tasks;
using PubDoomer.Tasks.Compile.Observables;

namespace PubDoomer.Tasks.Compile.Bcc;

public partial class ObservableBccCompileTask : CompileTaskBase
{
    public const string TaskName = "Compile (BCC)";
    private const string TaskDescription = "Compiles the ACS file from the given file path using a BCC compiler.";

    public override Type HandlerType => typeof(BccCompileTaskHandler);
    public override CompilerType Type => CompilerType.Bcc;
    public override string[] ExpectedFileExtensions { get; } = [".acs", ".bcs", ".txt"];

    [ObservableProperty] private ObservableCollection<ObservableString> _includeDirectories = new();
    
    // TODO: Implement additional parameters
    // [ObservableProperty] private bool _accErrorFile;
    // [ObservableProperty] private bool _accStats;
    // [ObservableProperty] private bool _help; // I don't think this one really makes sense.
    // [ObservableProperty] private bool _oneColumn;
    // [ObservableProperty] private int? _tabSize;
    // [ObservableProperty] private bool _stripAsserts;
    // [ObservableProperty] private bool _preprocessOnly;
    // [ObservableProperty] private Collection<string> _macros;
    // [ObservableProperty] private Collection<string> _linkLibraries;

    public ObservableBccCompileTask()
    {
    }

    public ObservableBccCompileTask(string? name, string? inputFilePath, string? outputFilePath, ObservableCollection<ObservableString>? includeDirectories = null)
        : base(name, inputFilePath, outputFilePath)
    {
        IncludeDirectories = includeDirectories ?? new();
    }

    public override string DisplayName => TaskName;
    public override string Description => TaskDescription;

    public override ObservableBccCompileTask DeepClone()
    {
        return new ObservableBccCompileTask(Name, InputFilePath, OutputFilePath, new(IncludeDirectories));
    }

    public override void Merge(ProjectTaskBase task)
    {
        if (task is not ObservableBccCompileTask bccCompileTask)
        {
            // TODO: Error?
            return;
        }

        Name = bccCompileTask.Name;
        InputFilePath = bccCompileTask.InputFilePath;
        OutputFilePath = bccCompileTask.OutputFilePath;
        GenerateStdOutAndStdErrFiles = bccCompileTask.GenerateStdOutAndStdErrFiles;
        IncludeDirectories = bccCompileTask.IncludeDirectories;
    }

    public override void Serialize(IProjectWriter writer)
    {
        writer.Write(IncludeDirectories.Count);
        foreach (var directory in IncludeDirectories.Where(x => !string.IsNullOrWhiteSpace(x.Value)))
        {
            writer.WritePath(directory.Value);
        }
        
        base.Serialize(writer);
    }

    public override void Deserialize(IProjectReader reader, ProjectSaveVersion version)
    {
        // Include directories was added in v0.2
        if (version >= new ProjectSaveVersion(0, 2))
        {
            var includedDirectoriesIterator = Enumerable.Range(0, reader.ReadInt32())
                .Select(x => reader.ReadPath())
                .OfType<string>()
                .Select(x => new ObservableString() { Value = x });

            IncludeDirectories = [.. includedDirectoriesIterator];
        }
        
        base.Deserialize(reader, version);
    }
}