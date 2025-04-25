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
    [ObservableProperty] private ObservableCollection<ObservableString> _macros = new();
    [ObservableProperty] private bool _stripAsserts = false;
    
    // TODO: Implement additional parameters
    
    // [ObservableProperty] private bool _oneColumn;
    // [ObservableProperty] private int? _tabSize;
    // [ObservableProperty] private bool _preprocessOnly;
    // [ObservableProperty] private Collection<string> _linkLibraries;
    
    // TODO: These three might need some special implementation.
    // [ObservableProperty] private bool _accErrorFile;
    // [ObservableProperty] private bool _accStats;
    // [ObservableProperty] private bool _help;

    public ObservableBccCompileTask()
    {
    }

    public ObservableBccCompileTask(string? name, string? inputFilePath, string? outputFilePath,
        ObservableCollection<ObservableString>? includeDirectories = null, ObservableCollection<ObservableString>? macros = null,
        bool stripAsserts = false)
        : base(name, inputFilePath, outputFilePath)
    {
        IncludeDirectories = includeDirectories ?? new();
        Macros = macros ?? new();
        StripAsserts = stripAsserts;
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
        Macros = bccCompileTask.Macros;
        StripAsserts = bccCompileTask.StripAsserts;
    }

    public override void Serialize(IProjectWriter writer)
    {
        // TODO: Counts need to also check agains null or empty.
        writer.Write(IncludeDirectories.Count);
        foreach (var directory in IncludeDirectories.Where(x => !string.IsNullOrWhiteSpace(x.Value)))
        {
            writer.Write(directory.Value);
        }
        
        writer.Write(Macros.Count);
        foreach (var macro in Macros.Where(x => !string.IsNullOrWhiteSpace(x.Value)))
        {
            writer.Write(macro.Value);
        }
        
        writer.Write(StripAsserts);
        base.Serialize(writer);
    }

    public override void Deserialize(IProjectReader reader, ProjectSaveVersion version)
    {
        // Include directories was added in v0.2
        if (version >= new ProjectSaveVersion(0, 2))
        {
            var includedDirectoriesIterator = Enumerable.Range(0, reader.ReadInt32())
                .Select(x => reader.ReadString())
                .Select(x => new ObservableString() { Value = x });

            IncludeDirectories = [.. includedDirectoriesIterator];
        }
        
        // Included macros and assertion stripping were added in v0.3
        if (version >= new ProjectSaveVersion(0, 3))
        {
            var macrosIterator = Enumerable.Range(0, reader.ReadInt32())
                .Select(x => reader.ReadString())
                .Select(x => new ObservableString() { Value = x });

            Macros = [.. macrosIterator];
            
            StripAsserts = reader.ReadBoolean();
        }
        
        base.Deserialize(reader, version);
    }
}