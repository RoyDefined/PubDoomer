using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Engine.Saving;
using PubDoomer.Project.Tasks;
using PubDoomer.Tasks.Compile.Observables;

namespace PubDoomer.Tasks.Compile.Acc;

public partial class ObservableAccCompileTask : CompileTaskBase
{
    public const string TaskName = "Compile (ACC)";
    private const string TaskDescription = "Compiles the ACS file from the given file path using an ACS compiler.";

    public override Type HandlerType => typeof(AccCompileTaskHandler);
    public override CompilerType Type => CompilerType.Acc;
    public override string[] ExpectedFileExtensions { get; } = [".acs", ".txt"];

    [ObservableProperty] private bool _keepAccErrFile;
    [ObservableProperty] private ObservableCollection<ObservableString> _includeDirectories = new();
    [ObservableProperty] private string? _debugFilePath;
    [ObservableProperty] private AccBytecodeCompatibilityLevel _bytecodeCompatibilityLevel;

    public ObservableAccCompileTask()
    {
    }

    public ObservableAccCompileTask(string? name, string? inputFilePath, string? outputFilePath, bool keepAccErrFile = false,
        ObservableCollection<ObservableString>? includeDirectories = null, string? debugFilePath = null, AccBytecodeCompatibilityLevel bytecodeCompatibilityLevel = AccBytecodeCompatibilityLevel.None)
        : base(name, inputFilePath, outputFilePath)
    {
        KeepAccErrFile = keepAccErrFile;
        IncludeDirectories = includeDirectories ?? new();
        DebugFilePath = debugFilePath;
        BytecodeCompatibilityLevel = bytecodeCompatibilityLevel;
    }

    public override string DisplayName => TaskName;
    public override string Description => TaskDescription;

    public override ObservableAccCompileTask DeepClone()
    {
        return new ObservableAccCompileTask(Name, InputFilePath, OutputFilePath, KeepAccErrFile, IncludeDirectories, DebugFilePath, BytecodeCompatibilityLevel);
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
        IncludeDirectories = accCompileTask.IncludeDirectories;
        DebugFilePath = accCompileTask.DebugFilePath;
        BytecodeCompatibilityLevel = accCompileTask.BytecodeCompatibilityLevel;
    }

    public override void Serialize(IProjectWriter writer)
    {
        base.Serialize(writer);
        writer.Write(KeepAccErrFile);
        
        // TODO: Counts need to also check agains null or empty.
        writer.Write(IncludeDirectories.Count);
        foreach (var directory in IncludeDirectories.Where(x => !string.IsNullOrWhiteSpace(x.Value)))
        {
            writer.Write(directory.Value);
        }
        
        writer.Write(DebugFilePath);
        writer.WriteEnum<AccBytecodeCompatibilityLevel>(BytecodeCompatibilityLevel);
    }

    public override void Deserialize(IProjectReader reader, ProjectSaveVersion version)
    {
        base.Deserialize(reader, version);
        KeepAccErrFile = reader.ReadBoolean();
        
        // Include directories and the debug file path were added in v0.4
        if (version >= new ProjectSaveVersion(0, 4))
        {
            var includedDirectoriesIterator = Enumerable.Range(0, reader.ReadInt32())
                .Select(x => reader.ReadString())
                .Select(x => new ObservableString() { Value = x });

            IncludeDirectories = [.. includedDirectoriesIterator];
            DebugFilePath = reader.ReadString();
            BytecodeCompatibilityLevel = reader.ReadEnum<AccBytecodeCompatibilityLevel>();
        }
    }
}