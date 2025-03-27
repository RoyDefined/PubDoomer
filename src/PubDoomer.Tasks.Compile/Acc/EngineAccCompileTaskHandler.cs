using Microsoft.Extensions.Logging;
using PubDoomer.Engine.Orchestration;
using PubDoomer.Engine.Process;
using PubDoomer.Engine.TaskHandling;
using PubDoomer.Engine.TaskInvokation.Context;
using PubDoomer.Tasks.Compile.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace PubDoomer.Tasks.Compile.Acc;

public sealed class EngineAccCompileTaskHandler(
    ILogger<EngineAccCompileTaskHandler> logger,
    EngineAccCompileTask taskInfo,
    TaskInvokeContext context) : ProcessInvokeHandlerBase(logger, taskInfo), ITaskHandler
{
    private readonly string _errorFilePath = Path.Combine(Path.GetDirectoryName(taskInfo.InputFilePath)!, "acs.err");
    private readonly CompositeFormat _errorFileRenamePathTemplate = CompositeFormat.Parse(Path.Combine(Path.GetDirectoryName(taskInfo.InputFilePath)!, "acs.err.backup{0}"));
    
    protected override string StdOutFileName => "stdout_acc.txt";
    protected override string StdErrFileName => "stderr_acc.txt";

    public async ValueTask<TaskInvokationResult> HandleAsync()
    {
        var path = context.ContextBag.GetAccCompilerExecutableFilePath();
        logger.LogDebug("Invoking {TaskName}. Input path: {InputFilePath}. Output path: {OutputFilePath}. Location of ACC executable: {AccExecutablePath}", nameof(EngineAccCompileTaskHandler), taskInfo.InputFilePath, taskInfo.OutputFilePath, path);

        // Check for existing 'acs.err' file.
        // If found, move this file because otherwise the compiler gets rid of it.
        if (File.Exists(_errorFilePath))
        {
            File.Move(_errorFilePath, GetValidBackupPath());
        }

        // Create streams for stdout and stderr if configured.
        // The stderr stream is optional unlike BCC for example, because compiler error details can be fetched from 'acs.err' instead.
        using var stdOutStream = taskInfo.GenerateStdOutAndStdErrFiles ? new MemoryStream() : null;
        using var stdErrStream = taskInfo.GenerateStdOutAndStdErrFiles ? new MemoryStream() : null;

        var result = await StartProcessAsync(
            new ProcessInvokeContext(path, BuildArguments(), stdOutStream, stdErrStream));

        // Premature exception was thrown, not related to compilation.
        // TODO: Continue if possible and check if we also have compiler errors, should the process have executed succesfully?
        if (result.Exception != null)
        {
            return TaskInvokationResult.FromError($"Compilation failed due to an error", null, result.Exception);
        }

        // Write stdout and stderr if configured.
        if (stdOutStream != null) await WriteStdOutToFileAsync(stdOutStream);
        if (stdErrStream != null) await WriteStdErrToFileAsync(stdErrStream);

        // The compiler returned an error.
        if (result.HasCompilerError)
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