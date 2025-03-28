using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using PubDoomer.Engine.TaskInvokation.Validation;
using PubDoomer.Tasks.Compile;

namespace PubDoomer.Project.Tasks;

public sealed class CompileTaskValidator(
    CompileTaskBase task) : ITaskValidator
{
    // TODO: Add validation for the output path
    public IEnumerable<ValidateResult> Validate()
    {
        // Check if path is set.
        if (string.IsNullOrWhiteSpace(task.InputFilePath))
        {
            yield return ValidateResult.FromError("File path is not provided.");
        }

        // Check if path contains invalid characters.
        if (task.InputFilePath != null && task.InputFilePath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
        {
            yield return ValidateResult.FromError("File path contains invalid characters.");
        }

        // Check if the path is a directory.
        if (Directory.Exists(task.InputFilePath))
        {
            yield return ValidateResult.FromWarning("The path might be a directory, not a file.");
        }

        // Check if the file exists
        if (!File.Exists(task.InputFilePath))
        {
            yield return ValidateResult.FromError("File to compile does not exist.");
        }

        // Check if the file has a valid extension
        var fileExtension = Path.GetExtension(task.InputFilePath);
        if (!task.ExpectedFileExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase))
        {
            yield return ValidateResult.FromWarning($"Unexpected file extension '{fileExtension}'. Expected: {string.Join(", ", task.ExpectedFileExtensions)}.");
        }
    }
}