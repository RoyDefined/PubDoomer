using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Engine.Tasks;

namespace PubDoomer.Project.Tasks;

public abstract partial class CompileTaskBase : ProjectTaskBase
{
    [ObservableProperty] private string? _inputFilePath;
    [ObservableProperty] private string? _outputFilePath;
    [ObservableProperty] private bool _generateStdOutAndStdErrFiles;

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
}