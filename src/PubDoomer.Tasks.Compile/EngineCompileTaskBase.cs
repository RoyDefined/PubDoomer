using PubDoomer.Engine.TaskInvokation.Task;
using PubDoomer.Engine.TaskInvokation.Validation;

namespace PubDoomer.Tasks.Compile;

public abstract class EngineCompileTaskBase : IRunnableTask
{
    public required string Name { get; init; }
    public required string InputFilePath { get; init; }
    public required string OutputFilePath { get; init; }
    public required bool GenerateStdOutAndStdErrFiles { get; init; }

    public abstract Type HandlerType { get; }
    public abstract CompilerType Type { get; }
    protected abstract string[] ExpectedFileExtensions { get; }
    
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
}