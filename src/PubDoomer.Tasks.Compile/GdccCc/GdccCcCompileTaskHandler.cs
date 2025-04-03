using Microsoft.Extensions.Logging;
using PubDoomer.Engine.TaskInvokation.Context;
using PubDoomer.Engine.TaskInvokation.Process;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using PubDoomer.Tasks.Compile.Extensions;
using System.Diagnostics;
using PubDoomer.Engine.Abstract;
using SystemProcess = System.Diagnostics.Process;

namespace PubDoomer.Tasks.Compile.GdccCc;

public sealed class GdccCcCompileTaskHandler(
    ILogger<GdccCcCompileTaskHandler> logger,
    ObservableGdccCcCompileTask taskInfo,
    TaskInvokeContext context) : ProcessInvokeHandlerBase(logger, taskInfo), ITaskHandler
{
    private const string CompileResultWarningPrefix = "WARNING: ";
    private const string CompileResultErrorPrefix = "ERROR: ";

    protected override string StdOutFileName => "stdout_gdcc-cc.txt";
    protected override string StdErrFileName => "stderr_gdcc-cc.txt";

    public async ValueTask<TaskInvokationResult> HandleAsync()
    {
        var gdccCcPath = context.ContextBag.GetGdccCcCompilerExecutableFilePath();
        var gdccMakeLibPath = context.ContextBag.GetGdccMakeLibCompilerExecutableFilePath();
        var gdccLdPath = context.ContextBag.GetGdccLdCompilerExecutableFilePath();

        logger.LogDebug("Invoking {TaskName}. Input path: {InputFilePath}. Output path: {OutputFilePath}. Location of GDCC-CC executable: {GdccCcExecutablePath}. Location of GDCC-MakeLib executable: {GdccMakeLibExecutablePath}. Location of GDCC-LD executable: {GdccLdExecutablePath}", nameof(GdccCcCompileTaskHandler), taskInfo.InputFilePath, taskInfo.OutputFilePath, gdccCcPath, gdccMakeLibPath, gdccLdPath);

        var libcOutputPath = Path.Combine(EngineStatics.TemporaryDirectory, "Gdcc-cc", "libGDCC.ir");
        var compiledOutputPath = Path.Combine(EngineStatics.TemporaryDirectory, "Gdcc-cc", "file.ir");

        // Compile libc, unless explicitly specified not to.
        await CompileLibcAsync(gdccMakeLibPath, libcOutputPath);

        // Compile the input files.
        await CompileAsync(gdccCcPath, libcOutputPath);

        // Link the files.
        await LinkAsync(gdccLdPath, libcOutputPath, compiledOutputPath);

        logger.LogDebug("GDCC-CC compilation and linking completed successfully.");
        return TaskInvokationResult.FromSuccess();
    }

    private async Task CompileAsync(string gdccCcPath, string libcOutputPath)
    {
        var args = BuildCompileArgs(libcOutputPath);
        var processStartInfo = new ProcessStartInfo
        {
            FileName = gdccCcPath,
            Arguments = string.Join(" ", args),
            UseShellExecute = false,
            CreateNoWindow = true
        };
        var makeLibProcess = new SystemProcess { StartInfo = processStartInfo };
        makeLibProcess.Start();
        await makeLibProcess.WaitForExitAsync();
    }

    private async Task CompileLibcAsync(string gdccMakeLibPath, string libcOutputPath)
    {
        if (taskInfo.DontBuildLibGdcc)
        {
            return;
        }

        var args = BuildMakeLibArgs(libcOutputPath);
        var processStartInfo = new ProcessStartInfo
        {
            FileName = gdccMakeLibPath,
            Arguments = string.Join(" ", args),
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new SystemProcess { StartInfo = processStartInfo };
        process.Start();
        await process.WaitForExitAsync();
    }

    private async Task LinkAsync(string gdccLdPath, string libcOutputPath, string compiledOutputPath)
    {
        var args = BuildLinkArgs(libcOutputPath, compiledOutputPath);
        var processStartInfo = new ProcessStartInfo
        {
            FileName = gdccLdPath,
            Arguments = string.Join(" ", args),
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new SystemProcess { StartInfo = processStartInfo };
        process.Start();
        await process.WaitForExitAsync();
    }

    private IEnumerable<string> BuildMakeLibArgs(string libcOutputPath)
    {
        yield return $"-co {libcOutputPath}";
        yield return $"libGDCC";

        if (taskInfo.TargetEngine != TargetEngineType.None)
        {
            yield return $"--target-engine {taskInfo.TargetEngine}";
        }
    }

    private IEnumerable<string> BuildCompileArgs(string libcOutputPath)
    {
        yield return $"-co file.ir";
        yield return taskInfo.InputFilePath!;

        if (taskInfo.TargetEngine != TargetEngineType.None)
        {
            yield return $"--target-engine {taskInfo.TargetEngine}";
        }
    }

    private IEnumerable<string> BuildLinkArgs(string libcOutputPath, string compiledOutputPath)
    {
        yield return $"-o {taskInfo.OutputFilePath}";
        yield return libcOutputPath;
        yield return compiledOutputPath;

        if (taskInfo.TargetEngine != TargetEngineType.None)
        {
            yield return $"--target-engine {taskInfo.TargetEngine}";
        }
    }
}