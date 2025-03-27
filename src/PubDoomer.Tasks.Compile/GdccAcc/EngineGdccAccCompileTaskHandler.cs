using Microsoft.Extensions.Logging;
using PubDoomer.Engine.Process;
using PubDoomer.Engine.TaskInvokation.Context;
using PubDoomer.Engine.TaskInvokation.Task;
using PubDoomer.Tasks.Compile.Extensions;
using System.Diagnostics;

namespace PubDoomer.Tasks.Compile.GdccAcc;

public sealed class EngineGdccAccCompileTaskHandler(
    ILogger<EngineGdccAccCompileTaskHandler> logger,
    GdccAccCompileTask taskInfo,
    TaskInvokeContext context) : ProcessInvokeHandlerBase(logger, taskInfo), ITaskHandler
{
    private const string CompileResultWarningPrefix = "WARNING: ";
    private const string CompileResultErrorPrefix = "ERROR: ";

    protected override string StdOutFileName => "stdout_gdccacc.txt";
    protected override string StdErrFileName => "stderr_gdccacc.txt";

    public async ValueTask<TaskInvokationResult> HandleAsync()
    {
        var path = context.ContextBag.GetGdccAccCompilerExecutableFilePath();
        logger.LogDebug("Invoking {TaskName}. Input path: {InputFilePath}. Output path: {OutputFilePath}. Location of GDCC-ACC executable: {GdccAccExecutablePath}", nameof(EngineGdccAccCompileTaskHandler), taskInfo.InputFilePath, taskInfo.OutputFilePath, path);

        // Create streams for stdout and stderr if configured.
        // The stderr stream is always created, as this gives us access to compile warnings and errors when they occured.
        using var stdOutStream = taskInfo.GenerateStdOutAndStdErrFiles ? new MemoryStream() : null;
        using var stdErrStream = new MemoryStream();

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
        if (taskInfo.GenerateStdOutAndStdErrFiles) await WriteStdErrToFileAsync(stdErrStream);

        // Process the error stream and filter out just the warnings.
        // Should the compiler have an error, we additionally filter out the errors.
        var results = await ProcessErrStreamAsync(stdErrStream);
        var warnings = results.Where(x => x.Type == GdccCompileResultType.Warning).Select(x => x.Message).ToArray();
        
        // The compiler returned an error.
        if (result.HasCompilerError)
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