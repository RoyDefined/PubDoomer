using Microsoft.Extensions.Logging;
using PubDoomer.Engine.TaskInvokation.Context;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using PubDoomer.Tasks.AcsVM.Extensions;
using System.Diagnostics;
using SystemProcess = System.Diagnostics.Process;

namespace PubDoomer.Tasks.AcsVM;

public sealed class AcsVirtualMachineExecuteTaskHandler(
    ILogger<AcsVirtualMachineExecuteTaskHandler> logger,
    AcsVirtualMachineExecuteTask taskInfo,
    TaskInvokeContext context) : ITaskHandler
{
    public async ValueTask<TaskInvokationResult> HandleAsync()
    {
        var path = context.ContextBag.GetAcsVmExecutableFilePath();
        logger.LogDebug("Invoking {TaskName}. Input path: {InputFilePath}. Location of ACSVM executable: {AccExecutablePath}", nameof(AcsVirtualMachineExecuteTaskHandler), taskInfo.InputFilePath, path);

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

        // The compiler returned an error.
        if (!succeeded)
        {
            // Try to get the stderr output.
            // TODO: Formatting of output should be improved once I find good examples on what can error.
            var errors = await ProcessErrStreamAsync(stdErrStream);
            
            if (errors.Length == 0)
            {
                return TaskInvokationResult.FromErrors("Code execution failed for unknown reason.");
            }

            return TaskInvokationResult.FromErrors($"Code execution failed.", null, errors);
        }

        // Get the result from the stream.
        // TODO: Make this on a per-line basis.
        using var reader = new StreamReader(stdOutStream);
        _ = stdOutStream.Seek(0, SeekOrigin.Begin);
        var codeResponse = await reader.ReadToEndAsync();
        
        return TaskInvokationResult.FromSuccess(codeResponse);
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

    private async Task<string[]> ProcessErrStreamAsync(Stream stream)
    {
        var results = new List<string>();

        await foreach (var result in ReadErrStreamAsync(stream))
        {
            results.Add(result);
        }

        return results.ToArray();
    }

    private async IAsyncEnumerable<string> ReadErrStreamAsync(Stream stream)
    {
        using var reader = new StreamReader(stream);
        _ = stream.Seek(0, SeekOrigin.Begin);

        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            yield return line;
        }
    }

    private IEnumerable<string> BuildArguments()
    {
        yield return $"\"{taskInfo.InputFilePath}\"";
    }
}