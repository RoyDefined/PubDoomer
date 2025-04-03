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
        // Validate target engine
        if (taskInfo.TargetEngine == TargetEngineType.None)
        {
            // TODO: Warn of missing engine.
            taskInfo.TargetEngine = TargetEngineType.Zandronum;
        }

        var gdccCcPath = context.ContextBag.GetGdccCcCompilerExecutableFilePath();
        var gdccMakeLibPath = context.ContextBag.GetGdccMakeLibCompilerExecutableFilePath();
        var gdccLdPath = context.ContextBag.GetGdccLdCompilerExecutableFilePath();

        logger.LogDebug("Invoking {TaskName}. Input path: {InputFilePath}. Output path: {OutputFilePath}.", nameof(GdccCcCompileTaskHandler), taskInfo.InputFilePath, taskInfo.OutputFilePath);

        var libcOutputPath = Path.Combine(EngineStatics.TemporaryDirectory, "Gdcc-cc", "libGDCC.ir");
        var compiledOutputPath = Path.Combine(EngineStatics.TemporaryDirectory, "Gdcc-cc", "file.ir");
        Directory.CreateDirectory(Path.Combine(EngineStatics.TemporaryDirectory, "Gdcc-cc"));

        // Compile libc, unless explicitly specified not to.
        if (!taskInfo.DontBuildLibGdcc)
        {
            logger.LogDebug("Compiling libc. Location of GDCC-MakeLib executable: {GdccMakeLibExecutablePath}.", gdccMakeLibPath);
            var makeLibResult = await CompileLibcAsync(gdccMakeLibPath, libcOutputPath);

            if (makeLibResult != 0)
            {
                return TaskInvokationResult.FromError("Compilation failed, process exit code was not 0.", null, null);
            }
        }

        // Compile the input files.
        logger.LogDebug("Compiling. Location of GDCC-CC executable: {GdccCcExecutablePath}.", gdccCcPath);
        var compileResult = await CompileAsync(gdccCcPath, compiledOutputPath);

        if (compileResult != 0)
        {
            return TaskInvokationResult.FromError("Compilation failed, process exit code was not 0.", null, null);
        }

        // Link the files.
        logger.LogDebug("Linking. Location of GDCC-LD executable: {GdccLdExecutablePath}", gdccLdPath);
        var linkResult = await LinkAsync(gdccLdPath, libcOutputPath, compiledOutputPath);

        if (linkResult != 0)
        {
            return TaskInvokationResult.FromError("Compilation failed, process exit code was not 0.", null, null);
        }

        logger.LogDebug("GDCC-CC compilation and linking completed successfully.");
        return TaskInvokationResult.FromSuccess();
    }

    private async Task<int> CompileLibcAsync(string gdccMakeLibPath, string libcOutputPath)
    {
        var args = BuildMakeLibArgs(libcOutputPath);
        var processStartInfo = new ProcessStartInfo
        {
            FileName = gdccMakeLibPath,
            Arguments = string.Join(" ", args),
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using var process = new SystemProcess { StartInfo = processStartInfo };
        process.Start();

        var stdout = await process.StandardOutput.ReadToEndAsync();
        var stderr = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        logger.LogDebug("GDCC-MakeLib stdout: {StdOut}", stdout);
        logger.LogDebug("GDCC-MakeLib stderr: {StdErr}", stderr);
        logger.LogDebug("Process exit code: {ExitCode}.", process.ExitCode);

        return process.ExitCode;
    }

    private async Task<int> CompileAsync(string gdccCcPath, string compiledOutputPath)
    {
        var args = BuildCompileArgs(compiledOutputPath);
        var processStartInfo = new ProcessStartInfo
        {
            FileName = gdccCcPath,
            Arguments = string.Join(" ", args),
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using var process = new SystemProcess { StartInfo = processStartInfo };
        process.Start();

        var stdout = await process.StandardOutput.ReadToEndAsync();
        var stderr = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        logger.LogDebug("GDCC-CC stdout: {StdOut}", stdout);
        logger.LogDebug("GDCC-CC stderr: {StdErr}", stderr);
        logger.LogDebug("Process exit code: {ExitCode}.", process.ExitCode);

        return process.ExitCode;
    }

    private async Task<int> LinkAsync(string gdccLdPath, string libcOutputPath, string compiledOutputPath)
    {
        var args = BuildLinkArgs(libcOutputPath, compiledOutputPath);
        var processStartInfo = new ProcessStartInfo
        {
            FileName = gdccLdPath,
            Arguments = string.Join(" ", args),
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using var process = new SystemProcess { StartInfo = processStartInfo };
        process.Start();

        var stdout = await process.StandardOutput.ReadToEndAsync();
        var stderr = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        logger.LogDebug("GDCC-LD stdout: {StdOut}", stdout);
        logger.LogDebug("GDCC-LD stderr: {StdErr}", stderr);
        logger.LogDebug("Process exit code: {ExitCode}.", process.ExitCode);

        return process.ExitCode;
    }

    private IEnumerable<string> BuildMakeLibArgs(string libcOutputPath)
    {
        yield return $"-co {libcOutputPath}";
        yield return "libGDCC";
        yield return "libc";
        yield return $"--target-engine {taskInfo.TargetEngine}";
    }

    private IEnumerable<string> BuildCompileArgs(string compiledOutputPath)
    {
        yield return $"-co {compiledOutputPath}";
        yield return taskInfo.InputFilePath!;
        yield return $"--target-engine {taskInfo.TargetEngine}";
    }

    private IEnumerable<string> BuildLinkArgs(string libcOutputPath, string compiledOutputPath)
    {
        yield return $"-o {taskInfo.OutputFilePath}";
        yield return libcOutputPath;
        yield return compiledOutputPath;
        yield return $"--target-engine {taskInfo.TargetEngine}";
    }
}
