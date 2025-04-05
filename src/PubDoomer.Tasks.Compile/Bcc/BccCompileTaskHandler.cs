using Microsoft.Extensions.Logging;
using PubDoomer.Engine.TaskInvokation.Context;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using PubDoomer.Tasks.Compile.Extensions;
using PubDoomer.Tasks.Compile.Utils;
using System.Diagnostics;
using SystemProcess = System.Diagnostics.Process;

namespace PubDoomer.Tasks.Compile.Bcc;

public sealed class BccCompileTaskHandler(
    ILogger<BccCompileTaskHandler> logger,
    ObservableBccCompileTask taskInfo,
    TaskInvokeContext context) : ITaskHandler
{
    public async ValueTask<TaskInvokationResult> HandleAsync()
    {
        var path = context.ContextBag.GetBccCompilerExecutableFilePath();
        logger.LogDebug("Invoking {TaskName}. Input path: {InputFilePath}. Output path: {OutputFilePath}. Location of BCC executable: {BccExecutablePath}", nameof(BccCompileTaskHandler), taskInfo.InputFilePath, taskInfo.OutputFilePath, path);

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