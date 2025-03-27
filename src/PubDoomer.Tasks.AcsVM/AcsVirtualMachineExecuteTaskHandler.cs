using Microsoft.Extensions.Logging;
using PubDoomer.Engine.Orchestration;
using PubDoomer.Engine.Process;
using PubDoomer.Engine.TaskHandling;
using PubDoomer.Engine.TaskInvokation.Context;
using PubDoomer.Engine.Tasks;
using PubDoomer.Tasks.Compile.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace PubDoomer.Engine.Tasks.AcsVM;

public sealed class AcsVirtualMachineExecuteTaskHandler(
    ILogger<AcsVirtualMachineExecuteTaskHandler> logger,
    AcsVirtualMachineExecuteTask taskInfo,
    TaskInvokeContext context) : ProcessInvokeHandlerBase(logger, taskInfo), ITaskHandler
{
    // Note, currently unused
    protected override string StdOutFileName => "stdout_acsvm.txt";
    protected override string StdErrFileName => "stderr_acsvm.txt";

    public async ValueTask<TaskInvokationResult> HandleAsync()
    {
        var path = context.ContextBag.GetAcsVmExecutableFilePath();
        logger.LogDebug("Invoking {TaskName}. Input path: {InputFilePath}. Location of ACSVM executable: {AccExecutablePath}", nameof(AcsVirtualMachineExecuteTaskHandler), taskInfo.InputFilePath, path);

        // We make an stdout and stderr stream regardless of configuration.
        using var stdOutStream = new MemoryStream();
        using var stdErrStream = new MemoryStream();

        var result = await StartProcessAsync(
            new ProcessInvokeContext(path, BuildArguments(), stdOutStream, stdErrStream));

        // Premature exception was thrown, not related to compilation.
        // TODO: Continue if possible and check if we also have compiler errors, should the process have executed succesfully?
        if (result.Exception != null)
        {
            return TaskInvokationResult.FromError("Code execution failed due to an error", null, result.Exception);
        }

        // The compiler returned an error.
        if (result.HasCompilerError)
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