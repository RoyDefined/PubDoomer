using Microsoft.Extensions.Logging;
using PubDoomer.Engine.Process;
using PubDoomer.Engine.TaskInvokation.Context;
using PubDoomer.Engine.TaskInvokation.Task;
using PubDoomer.Tasks.Compile.Extensions;
using System.Diagnostics;

namespace PubDoomer.Tasks.Compile.Bcc;

public sealed class EngineBccCompileTaskHandler(
    ILogger<EngineBccCompileTaskHandler> logger,
    EngineBccCompileTask taskInfo,
    TaskInvokeContext context) : ProcessInvokeHandlerBase(logger, taskInfo), ITaskHandler
{
    protected override string StdOutFileName => "stdout_bcc.txt";
    protected override string StdErrFileName => "stderr_bcc.txt";

    public async ValueTask<TaskInvokationResult> HandleAsync()
    {
        var path = context.ContextBag.GetBccCompilerExecutableFilePath();
        logger.LogDebug("Invoking {TaskName}. Input path: {InputFilePath}. Output path: {OutputFilePath}. Location of BCC executable: {BccExecutablePath}", nameof(EngineBccCompileTaskHandler), taskInfo.InputFilePath, taskInfo.OutputFilePath, path);

        // Create streams for stdout and stderr if configured.
        // The stderr stream is always created, as this gives us access to compile errors when they occured.
        using var stdOutStream = taskInfo.GenerateStdOutAndStdErrFiles ? new MemoryStream() : null;
        using var stdErrStream = new MemoryStream();

        var result = await StartProcessAsync(
            new ProcessInvokeContext(path, BuildArguments(), stdOutStream, stdErrStream));

        // Premature exception was thrown, not related to compilation.
        // TODO: Continue if possible and check if we also have compiler errors, should the process have executed?
        if (result.Exception != null)
        {
            return TaskInvokationResult.FromError($"Compilation failed due to an error", null, result.Exception);
        }

        // Write stdout and stderr if configured.
        if (stdOutStream != null) await WriteStdOutToFileAsync(stdOutStream);
        if (taskInfo.GenerateStdOutAndStdErrFiles) await WriteStdErrToFileAsync(stdErrStream);

        // The compiler returned an error.
        if (result.HasCompilerError)
        {
            // Check stderr for content, which means compilation failed.
            var compileResult = await HandleCompileErrorAsync(stdErrStream);
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

    private async ValueTask<string?> HandleCompileErrorAsync(Stream stream)
    {
        _ = stream.Seek(0, SeekOrigin.Begin);

        string content;
        using (var reader = new StreamReader(stream))
        {
            content = await reader.ReadToEndAsync();
        }

        // If the content is empty, there was no error.
        if (string.IsNullOrEmpty(content))
        {
            return null;
        }
        
        return content;
    }
}