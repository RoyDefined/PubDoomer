using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Engine.Saving;
using PubDoomer.Project.Tasks;
using PubDoomer.Tasks.Compile.Observables;

namespace PubDoomer.Tasks.Compile.GdccAcc;

public partial class ObservableGdccAccCompileTask : CompileTaskBase
{
    public const string TaskName = "Compile (GDCC-ACC)";
    private const string TaskDescription = "Compiles the ACS file from the given file path using a GDCC-ACC compiler.";

    public override Type HandlerType => typeof(GdccAccCompileTaskHandler);
    public override CompilerType Type => CompilerType.GdccAcc;
    public override string[] ExpectedFileExtensions { get; } = [".acs", ".txt"];

    [ObservableProperty] private bool _dontWarnForwardReferences;
    [ObservableProperty] private ObservableCollection<ObservableString> _includeDirectories = new();
    [ObservableProperty] private ObservableCollection<ObservableString> _macros = new();

    // TODO: Implement additional parameters.

    public ObservableGdccAccCompileTask()
    {
    }

    public ObservableGdccAccCompileTask(string? name, string? inputFilePath, string? outputFilePath, bool dontWarnForwardReferences = false,
        ObservableCollection<ObservableString>? includeDirectories = null, ObservableCollection<ObservableString>? macros = null)
        : base(name, inputFilePath, outputFilePath)
    {
        DontWarnForwardReferences = dontWarnForwardReferences;
        IncludeDirectories = includeDirectories ?? new();
        Macros = macros ?? new();
    }

    public override string DisplayName => TaskName;
    public override string Description => TaskDescription;

    public override ObservableGdccAccCompileTask DeepClone()
    {
        return new ObservableGdccAccCompileTask(Name, InputFilePath, OutputFilePath, DontWarnForwardReferences, IncludeDirectories, Macros);
    }

    public override void Merge(ProjectTaskBase task)
    {
        if (task is not ObservableGdccAccCompileTask gdccAccCompileTask)
        {
            // TODO: Error?
            return;
        }

        Name = gdccAccCompileTask.Name;
        InputFilePath = gdccAccCompileTask.InputFilePath;
        OutputFilePath = gdccAccCompileTask.OutputFilePath;
        GenerateStdOutAndStdErrFiles = gdccAccCompileTask.GenerateStdOutAndStdErrFiles;
        DontWarnForwardReferences = gdccAccCompileTask.DontWarnForwardReferences;
        IncludeDirectories = gdccAccCompileTask.IncludeDirectories;
        Macros = gdccAccCompileTask.Macros;
    }

    public override void Serialize(IProjectWriter writer)
    {
        base.Serialize(writer);
        writer.Write(DontWarnForwardReferences);
        
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
        DontWarnForwardReferences = reader.ReadBoolean();
        
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