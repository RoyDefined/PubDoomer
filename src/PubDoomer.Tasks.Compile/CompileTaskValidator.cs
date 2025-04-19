using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Engine.TaskInvokation.Context;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using PubDoomer.Engine.TaskInvokation.Validation;
using PubDoomer.Tasks.Compile;

namespace PubDoomer.Project.Tasks;

public sealed class CompileTaskValidator(
    CompileTaskBase task) : ITaskValidator
{
    // TODO: Add validation for the output path
    public IEnumerable<ValidateResult> Validate(TaskInvokeContext invokeContext)
    {
        var inputPath = GetArgument(invokeContext);

        if (string.IsNullOrWhiteSpace(inputPath))
        {
            yield return ValidateResult.FromError("File path is not provided.");
        }
        else
        {
            if (inputPath.IndexOfAny(Path.GetInvalidPathChars()) >= 0) yield return ValidateResult.FromError("File path contains invalid characters.");
            if (Directory.Exists(inputPath)) yield return ValidateResult.FromWarning("The path might be a directory, not a file.");
            if (!File.Exists(inputPath)) yield return ValidateResult.FromError("File to compile does not exist.");

            var fileExtension = Path.GetExtension(inputPath);
            if (!task.ExpectedFileExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase))
                yield return ValidateResult.FromWarning($"Unexpected file extension '{fileExtension}'. Expected: {string.Join(", ", task.ExpectedFileExtensions)}.");
        }
    }

    private string GetArgument(TaskInvokeContext invokeContext)
    {
        var inputPath = task.InputFilePath;
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath, nameof(task.InputFilePath));

        // Handle relative input path
        if (!Path.IsPathRooted(inputPath))
        {
            if (string.IsNullOrWhiteSpace(invokeContext.WorkingDirectory))
            {
                throw new ArgumentException($"Failed to update relative input path for ACS file input ({inputPath}). No working directory was specified. Either the working directory must be specified or the path to the ACS file input must be absolute.");
            }

            inputPath = Path.Combine(invokeContext.WorkingDirectory, inputPath);
        }

        return inputPath;
    }
}