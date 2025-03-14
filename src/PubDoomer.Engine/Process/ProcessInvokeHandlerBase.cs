using System.Diagnostics;
using Microsoft.Extensions.Logging;
using PubDoomer.Engine.Orchestration;
using PubDoomer.Engine.Static;
using SystemProcess = System.Diagnostics.Process;

namespace PubDoomer.Engine.Process;

public abstract class ProcessInvokeHandlerBase(
    ILogger<ProcessInvokeHandlerBase> logger, EngineTaskBase taskInfo)
{
    protected abstract string StdOutFileName { get; }
    protected abstract string StdErrFileName { get; }

    private protected string LogDirectory => Path.Combine(EngineStatics.TemporaryDirectory, taskInfo.Name);
    private protected string StdOutFilePath => Path.Combine(LogDirectory, StdOutFileName);
    private protected string StdErrFilePath => Path.Combine(LogDirectory, StdErrFileName);

    protected async Task<ProcessInvokeResult> StartProcessAsync(ProcessInvokeContext context)
    {
        try
        {
            return ProcessInvokeResult.Create(
                await StartProcessCoreAsync(context));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occured during the process.");
            return ProcessInvokeResult.Create(ex);
        }
    }

    private async Task<int> StartProcessCoreAsync(ProcessInvokeContext context)
    {
        logger.LogDebug("calling process.");

        var processStartInfo = new ProcessStartInfo
        {
            FileName = context.ExeFilePath,
            Arguments = string.Join(" ", context.Arguments),
            RedirectStandardOutput = context.stdOutStream != null,
            RedirectStandardError = context.stdErrStream != null,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new SystemProcess { StartInfo = processStartInfo };
        process.Start();

        // Output stdout and stderr if configured.
        var outputTask = context.stdOutStream != null
            ? process.StandardOutput.BaseStream.CopyToAsync(context.stdOutStream)
            : Task.CompletedTask;
        var errorTask = context.stdErrStream != null
            ? process.StandardError.BaseStream.CopyToAsync(context.stdErrStream)
            : Task.CompletedTask;

        await Task.WhenAll(outputTask, errorTask, process.WaitForExitAsync());

        logger.LogDebug("Process exit code: {ExitCode}", process.ExitCode);
        return process.ExitCode;
    }

    protected async Task WriteStdOutToFileAsync(Stream stream)
    {
        logger.LogDebug("Writing stdout. File path: {StdOutFilePath}", StdOutFilePath);

        EnsureStreamFileExists();
        _ = stream.Seek(0, SeekOrigin.Begin);

        using var fileStream = new FileStream(StdOutFilePath, FileMode.Create, FileAccess.Write);
        await stream.CopyToAsync(fileStream);
    }

    protected async Task WriteStdErrToFileAsync(Stream stream)
    {
        logger.LogDebug("Writing stderr. File path: {StdErrFilePath}", StdErrFilePath);

        EnsureStreamFileExists();
        _ = stream.Seek(0, SeekOrigin.Begin);

        using var fileStream = new FileStream(StdErrFilePath, FileMode.Create, FileAccess.Write);
        await stream.CopyToAsync(fileStream);
    }

    private void EnsureStreamFileExists()
    {
        _ = Directory.CreateDirectory(LogDirectory);
    }
}
