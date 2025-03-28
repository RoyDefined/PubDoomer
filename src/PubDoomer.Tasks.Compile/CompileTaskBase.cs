using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using PubDoomer.Engine.TaskInvokation.Validation;
using PubDoomer.Tasks.Compile;

namespace PubDoomer.Project.Tasks;

public abstract partial class CompileTaskBase : ProjectTaskBase, IRunnableTask, IValidatableTask
{
    [ObservableProperty] private string? _inputFilePath;
    [ObservableProperty] private string? _outputFilePath;
    [ObservableProperty] private bool _generateStdOutAndStdErrFiles;

    public abstract Type HandlerType { get; }
    public abstract CompilerType Type { get; }
    protected abstract string[] ExpectedFileExtensions { get; }

    public CompileTaskBase()
    {
    }

    public CompileTaskBase(string? name, string? inputFilePath, string? outputFilePath)
    {
        Name = name;
        InputFilePath = inputFilePath;
        OutputFilePath = outputFilePath;
    }

    // TODO: Add validation for the output path
    public IEnumerable<ValidateResult> Validate()
    {
        // Check if path is set.
        if (string.IsNullOrWhiteSpace(InputFilePath))
        {
            yield return ValidateResult.FromError("File path is not provided.");
        }

        // Check if path contains invalid characters.
        if (InputFilePath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
        {
            yield return ValidateResult.FromError("File path contains invalid characters.");
        }

        // Check if the path is a directory.
        if (Directory.Exists(InputFilePath))
        {
            yield return ValidateResult.FromWarning("The path might be a directory, not a file.");
        }

        // Check if the file exists
        if (!File.Exists(InputFilePath))
        {
            yield return ValidateResult.FromError("File to compile does not exist.");
        }

        // Check if the file has a valid extension
        var fileExtension = Path.GetExtension(InputFilePath);
        if (!ExpectedFileExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase))
        {
            yield return ValidateResult.FromWarning($"Unexpected file extension '{fileExtension}'. Expected: {string.Join(", ", ExpectedFileExtensions)}.");
        }
    }

    public abstract override CompileTaskBase DeepClone();
}