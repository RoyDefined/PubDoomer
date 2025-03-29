using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Engine.Saving;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using PubDoomer.Engine.TaskInvokation.Validation;
using PubDoomer.Tasks.Compile;

namespace PubDoomer.Project.Tasks;

public abstract partial class CompileTaskBase : ProjectTaskBase
{
    [ObservableProperty] private string? _inputFilePath;
    [ObservableProperty] private string? _outputFilePath;
    [ObservableProperty] private bool _generateStdOutAndStdErrFiles;

    public abstract CompilerType Type { get; }
    public abstract string[] ExpectedFileExtensions { get; }
    public override Type ValidatorType => typeof(CompileTaskValidator);

    public CompileTaskBase()
    {
    }

    public CompileTaskBase(string? name, string? inputFilePath, string? outputFilePath)
    {
        Name = name;
        InputFilePath = inputFilePath;
        OutputFilePath = outputFilePath;
    }

    public abstract override CompileTaskBase DeepClone();

    public override void Serialize(IProjectWriter writer)
    {
        writer.WritePath(InputFilePath ?? string.Empty);
        writer.WritePath(OutputFilePath ?? string.Empty);
        writer.Write(GenerateStdOutAndStdErrFiles);
    }

    public override void Deserialize(IProjectReader reader)
    {
        InputFilePath = reader.ReadString();
        OutputFilePath = reader.ReadString();
        GenerateStdOutAndStdErrFiles = reader.ReadBoolean();
    }
}