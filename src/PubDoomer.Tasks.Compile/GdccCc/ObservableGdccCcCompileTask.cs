using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Engine.Saving;
using PubDoomer.Project.Tasks;
using PubDoomer.Tasks.Compile.GdccCc;
using PubDoomer.Tasks.Compile.Observables;

namespace PubDoomer.Tasks.Compile.GdccCc;

public partial class ObservableGdccCcCompileTask : CompileTaskBase
{
    public const string TaskName = "Compile (GDCC-CC)";
    private const string TaskDescription = "Compiles the C file from the given file path using a GDCC-CC compiler.";

    public override Type HandlerType => typeof(GdccCcCompileTaskHandler);
    public override CompilerType Type => CompilerType.GdccCc;
    public override string[] ExpectedFileExtensions { get; } = [".c", ".txt"];
    
    /// <summary>
    /// If <c>true</c>, build and link libc with the compiled file.
    /// </summary>
    [ObservableProperty] private bool _linkLibc = true;
    
    /// <summary>
    /// If <c>true</c>, build and link libGDCC with the compiled file.
    /// </summary>
    [ObservableProperty] private bool _linkLibGdcc = true;

    [ObservableProperty] private TargetEngineType _targetEngine;
    [ObservableProperty] private ObservableCollection<ObservableString> _includeDirectories = new();
    [ObservableProperty] private ObservableCollection<ObservableString> _macros = new();

    // TODO: Implement additional parameters.

    public ObservableGdccCcCompileTask()
    {
    }

    public ObservableGdccCcCompileTask(
        string? name, string? inputFilePath, string? outputFilePath, bool linkLibc = true, bool linkLibGdcc = true,
        TargetEngineType targetEngine = TargetEngineType.None,
        ObservableCollection<ObservableString>? includeDirectories = null, ObservableCollection<ObservableString>? macros = null)
        : base(name, inputFilePath, outputFilePath)
    {
        LinkLibc = linkLibc;
        LinkLibGdcc = linkLibGdcc;
        TargetEngine = targetEngine;
        IncludeDirectories = includeDirectories ?? new();
        Macros = macros ?? new();
    }

    public override string DisplayName => TaskName;
    public override string Description => TaskDescription;

    public override ObservableGdccCcCompileTask DeepClone()
    {
        return new ObservableGdccCcCompileTask(
            Name, InputFilePath, OutputFilePath,
            LinkLibc, LinkLibGdcc, TargetEngine,
            IncludeDirectories, Macros);
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
        LinkLibc = gdccCcCompileTask.LinkLibc;
        LinkLibGdcc = gdccCcCompileTask.LinkLibGdcc;
        TargetEngine = gdccCcCompileTask.TargetEngine;
        IncludeDirectories = gdccCcCompileTask.IncludeDirectories;
        Macros = gdccCcCompileTask.Macros;
    }

    public override void Serialize(IProjectWriter writer)
    {
        // TODO: Target engine should come after the linking
        base.Serialize(writer);
        writer.WriteEnum<TargetEngineType>(TargetEngine);
        writer.Write(LinkLibc);
        writer.Write(LinkLibGdcc);
        
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
    }

    public override void Deserialize(IProjectReader reader, ProjectSaveVersion version)
    {
        base.Deserialize(reader, version);
        TargetEngine = reader.ReadEnum<TargetEngineType>();
        LinkLibc = reader.ReadBoolean();
        LinkLibGdcc = reader.ReadBoolean();
        
        // Include directories and macros were added in v0.4
        if (version >= new ProjectSaveVersion(0, 2))
        {
            var includedDirectoriesIterator = Enumerable.Range(0, reader.ReadInt32())
                .Select(x => reader.ReadString())
                .Select(x => new ObservableString() { Value = x });

            IncludeDirectories = [.. includedDirectoriesIterator];
            
            var macrosIterator = Enumerable.Range(0, reader.ReadInt32())
                .Select(x => reader.ReadString())
                .Select(x => new ObservableString() { Value = x });

            Macros = [.. macrosIterator];
        }
    }
}