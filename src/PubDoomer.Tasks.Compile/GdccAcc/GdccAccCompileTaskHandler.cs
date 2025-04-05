using Microsoft.Extensions.Logging;
using PubDoomer.Engine.TaskInvokation.Context;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using PubDoomer.Tasks.Compile.Extensions;
using PubDoomer.Tasks.Compile.Utils;
using System.Diagnostics;
using SystemProcess = System.Diagnostics.Process;

namespace PubDoomer.Tasks.Compile.GdccAcc;

public sealed class GdccAccCompileTaskHandler(
    ILogger<GdccAccCompileTaskHandler> logger,
    ObservableGdccAccCompileTask taskInfo,
    TaskInvokeContext context) : ITaskHandler
{
    private const string CompileResultWarningPrefix = "WARNING: ";
    private const string CompileResultErrorPrefix = "ERROR: ";

    public async ValueTask<TaskInvokationResult> HandleAsync()
    {
        var path = context.ContextBag.GetGdccAccCompilerExecutableFilePath();
        logger.LogDebug("Invoking {TaskName}. Input path: {InputFilePath}. Output path: {OutputFilePath}. Location of GDCC-ACC executable: {GdccAccExecutablePath}", nameof(GdccAccCompileTaskHandler), taskInfo.InputFilePath, taskInfo.OutputFilePath, path);

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

        // Process the error stream and filter out just the warnings.
        // Should the compiler have an error, we additionally filter out the errors.
        var results = await ProcessErrStreamAsync(stdErrStream);
        var warnings = results.Where(x => x.Type == GdccCompileResultType.Warning).Select(x => x.Message).ToArray();

        // The compiler returned an error.
        if (!succeeded)
        {
            var errors = results.Where(x => x.Type == GdccCompileResultType.Error).Select(x => x.Message).ToArray();

            if (errors.Length == 0)
            {
                return TaskInvokationResult.FromErrors("Compilation failed for unknown reason.", warnings);
            }

            return TaskInvokationResult.FromErrors($"Compilation failed", warnings, errors);
        }

        var message = warnings.Length != 0
            ? $"Compiled succesfully with {warnings.Length} warning{(warnings.Length == 1 ? string.Empty : "s")}."
            : "Compiled succesfully.";
        return TaskInvokationResult.FromSuccess(message, warnings);
    }

    private IEnumerable<string> BuildArguments()
    {
        yield return $"\"{taskInfo.InputFilePath}\"";
        yield return $"-o \"{taskInfo.OutputFilePath}\"";

        if (taskInfo.DontWarnForwardReferences)
        {
            yield return "--no-warn-forward-reference";
        }
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

    private async Task<GdccCompileResult[]> ProcessErrStreamAsync(Stream stream)
    {
        var results = new List<GdccCompileResult>();

        await foreach (var result in ReadErrStreamAsync(stream))
        {
            results.Add(result);
        }

        return results.ToArray();
    }

    private async IAsyncEnumerable<GdccCompileResult> ReadErrStreamAsync(Stream stream)
    {
        using var reader = new StreamReader(stream);
        _ = stream.Seek(0, SeekOrigin.Begin);

        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            var result = ParseLine(line);
            if (result == null)
            {
                logger.LogWarning("Encountered an invalid line in compile results: '{Line}'", line);
                continue;
            }

            yield return result.Value;
        }
    }

    private static GdccCompileResult? ParseLine(string line)
    {
        return line switch
        {
            { } when line.StartsWith(CompileResultWarningPrefix) => new GdccCompileResult(GdccCompileResultType.Warning, line[CompileResultWarningPrefix.Length..]),
            { } when line.StartsWith(CompileResultErrorPrefix) => new GdccCompileResult(GdccCompileResultType.Error, line[CompileResultErrorPrefix.Length..]),
            _ => null
        };
    }
}