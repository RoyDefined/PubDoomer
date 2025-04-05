using Microsoft.Extensions.Logging;
using PubDoomer.Engine.TaskInvokation.Context;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using PubDoomer.Tasks.Compile.Extensions;
using PubDoomer.Tasks.Compile.Utils;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using SystemProcess = System.Diagnostics.Process;

namespace PubDoomer.Tasks.Compile.Acc;

public sealed class AccCompileTaskHandler(
    ILogger<AccCompileTaskHandler> logger,
    ObservableAccCompileTask taskInfo,
    TaskInvokeContext context) : ITaskHandler
{
    private readonly string _errorFilePath = Path.Combine(Path.GetDirectoryName(taskInfo.InputFilePath)!, "acs.err");
    private readonly CompositeFormat _errorFileRenamePathTemplate = CompositeFormat.Parse(Path.Combine(Path.GetDirectoryName(taskInfo.InputFilePath)!, "acs.err.backup{0}"));

    public async ValueTask<TaskInvokationResult> HandleAsync()
    {
        var path = context.ContextBag.GetAccCompilerExecutableFilePath();
        logger.LogDebug("Invoking {TaskName}. Input path: {InputFilePath}. Output path: {OutputFilePath}. Location of ACC executable: {AccExecutablePath}", nameof(AccCompileTaskHandler), taskInfo.InputFilePath, taskInfo.OutputFilePath, path);

        // Verify the task has a name.
        if (taskInfo.Name == null)
        {
            return TaskInvokationResult.FromError("Task is missing a name.");
        }

        // Check for existing 'acs.err' file.
        // If found, move this file because otherwise the compiler gets rid of it.
        if (File.Exists(_errorFilePath))
        {
            File.Move(_errorFilePath, GetValidBackupPath());
        }

        // Create streams for stdout and stderr.
        using var stdOutStream = new MemoryStream();
        using var stdErrStream = new MemoryStream();

        bool succeeded;
        try
        {
            succeeded = await StartProcessAsync(path, BuildArguments(), stdOutStream, stdErrStream);
        }

        // Premature exception was thrown, not related to compilation.
        // TODO: Continue if possible and check if we also have compiler errors, should the process have executed succesfully?
        catch (Exception ex)
        {
            return TaskInvokationResult.FromError("Code execution failed due to an error", null, ex);
        }

        // Write stdout and stderr if configured.
        if (taskInfo.GenerateStdOutAndStdErrFiles)
        {
            await TaskHelper.WriteToFileAsync(stdOutStream, taskInfo.Name, "stdout.txt");
            await TaskHelper.WriteToFileAsync(stdErrStream, taskInfo.Name, "stderr.txt");
        }

        // The compiler returned an error.
        if (!succeeded)
        {
            // Check for `acs.err`. Otherwise give a generic error.
            var compileResult = await HandleAccErrAsync();
            if (compileResult == null)
            {
                return TaskInvokationResult.FromError("Compilation failed for unknown reason.");
            }

            return TaskInvokationResult.FromError($"Compilation failed", compileResult);
        }

        return TaskInvokationResult.FromSuccess();
    }

    private IEnumerable<string> BuildArguments()
    {
        yield return $"\"{taskInfo.InputFilePath}\"";
        yield return $"\"{taskInfo.OutputFilePath}\"";
    }

    private async Task<bool> StartProcessAsync(string path, IEnumerable<string> arguments, MemoryStream stdOutStream, MemoryStream stdErrStream)
    {
        logger.LogDebug("calling process.");

        var processStartInfo = new ProcessStartInfo
        {
            FileName = path,
            Arguments = string.Join(" ", arguments),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new SystemProcess { StartInfo = processStartInfo };
        process.Start();

        // Additional task to output stdout and stderr.
        var outputTask = process.StandardOutput.BaseStream.CopyToAsync(stdOutStream);
        var errorTask = process.StandardError.BaseStream.CopyToAsync(stdErrStream);
        await Task.WhenAll(outputTask, errorTask, process.WaitForExitAsync());

        logger.LogDebug("Process exit code: {ExitCode}", process.ExitCode);
        return process.ExitCode == 0;
    }

    private string GetValidBackupPath()
    {
        var path = string.Format(CultureInfo.InvariantCulture, _errorFileRenamePathTemplate, string.Empty);
        var nextFileIndex = 0;
        while (File.Exists(path))
        {
            nextFileIndex++;
            path = string.Format(CultureInfo.InvariantCulture, _errorFileRenamePathTemplate, $" ({nextFileIndex})");
        }

        return path;
    }

    private async ValueTask<string?> HandleAccErrAsync()
    {
        if (!File.Exists(_errorFilePath))
        {
            return null;
        }

        string content;
        using (var reader = new StreamReader(_errorFilePath))
        {
            content = await reader.ReadToEndAsync();
        }

        // If the error file should not be kept then remove the 'acs.err' file.
        if (!taskInfo.KeepAccErrFile)
        {
            logger.LogDebug("Remove 'acs.err' file as configured.");
            File.Delete(_errorFilePath);
        }

        return content;
    }
}