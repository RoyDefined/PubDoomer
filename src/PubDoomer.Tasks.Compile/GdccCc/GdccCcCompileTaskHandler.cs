using Microsoft.Extensions.Logging;
using PubDoomer.Engine.TaskInvokation.Context;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using PubDoomer.Tasks.Compile.Extensions;
using System.Diagnostics;
using PubDoomer.Engine.Abstract;
using SystemProcess = System.Diagnostics.Process;
using PubDoomer.Engine.TaskInvokation.Orchestration;
using PubDoomer.Tasks.Compile.GdccAcc;

namespace PubDoomer.Tasks.Compile.GdccCc;

public sealed class GdccCcCompileTaskHandler : ITaskHandler
{
    private static readonly string _tempDirectory = Path.Combine(EngineStatics.TemporaryDirectory, "Gdcc-cc");
    private static readonly string _makeLibOutputPath = Path.Combine(_tempDirectory, "makelibfile.ir");
    private static readonly string _compiledOutputPath = Path.Combine(_tempDirectory, "file.ir");

    private readonly ILogger _logger;
    private readonly IInvokableTask _taskContext;
    private readonly TaskInvokeContext _invokeContext;
    private readonly ObservableGdccCcCompileTask _task;

    public GdccCcCompileTaskHandler(
        ILogger<GdccCcCompileTaskHandler> logger, IInvokableTask taskContext, TaskInvokeContext invokeContext)
    {
        if (taskContext.Task is not ObservableGdccCcCompileTask task)
        {
            throw new ArgumentException($"The given task is not a {nameof(ObservableGdccCcCompileTask)}.");
        }

        _logger = logger;
        _taskContext = taskContext;
        _invokeContext = invokeContext;
        _task = task;
    }

    public async ValueTask<bool> HandleAsync()
    {
        // Validate target engine
        if (_task.TargetEngine == TargetEngineType.None)
        {
            // TODO: Warn of missing engine.
            _task.TargetEngine = TargetEngineType.Zandronum;
        }
        
        Directory.CreateDirectory(_tempDirectory);

        var gdccCcPath = _invokeContext.ContextBag.GetGdccCcCompilerExecutableFilePath();
        var gdccMakeLibPath = _invokeContext.ContextBag.GetGdccMakeLibCompilerExecutableFilePath();
        var gdccLdPath = _invokeContext.ContextBag.GetGdccLdCompilerExecutableFilePath();

        _logger.LogDebug("Invoking {TaskName}. Input path: {InputFilePath}. Output path: {OutputFilePath}.", nameof(GdccCcCompileTaskHandler), _task.InputFilePath, _task.OutputFilePath);

        // Compile libc and/or libGDCC, unless explicitly specified not to.
        // This setting is merged as to call the process once with both parameters.
        if (_task.LinkLibc || _task.LinkLibGdcc)
        {
            _logger.LogDebug("Compiling libc and/or libGDCC. Location of GDCC-MakeLib executable: {GdccMakeLibExecutablePath}. Compile Libc: {CompileLibc}. Compile LibGDCC: {CompileLibGdcc}.", gdccMakeLibPath, _task.LinkLibc, _task.LinkLibGdcc);
            var makeLibResult = await MakeLibAsync(gdccMakeLibPath);

            if (makeLibResult != 0)
            {
                _taskContext.TaskOutput.Add(TaskOutputResult.CreateError("Compilation failed, process exit code was not 0."));
                return false;
            }
        }

        // Compile the input files.
        _logger.LogDebug("Compiling. Location of GDCC-CC executable: {GdccCcExecutablePath}.", gdccCcPath);
        var compileResult = await CompileAsync(gdccCcPath);

        if (compileResult != 0)
        {
            _taskContext.TaskOutput.Add(TaskOutputResult.CreateError("Compilation failed, process exit code was not 0."));
            return false;
        }

        // Link the files.
        _logger.LogDebug("Linking. Location of GDCC-LD executable: {GdccLdExecutablePath}", gdccLdPath);
        var linkResult = await LinkAsync(gdccLdPath);

        if (linkResult != 0)
        {
            _taskContext.TaskOutput.Add(TaskOutputResult.CreateError("Compilation failed, process exit code was not 0."));
            return false;
        }

        _logger.LogDebug("GDCC-CC compilation and linking completed successfully.");
        return true;
    }

    private async Task<int> MakeLibAsync(string gdccMakeLibPath)
    {
        var args = BuildMakeLibArgs();
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

        _logger.LogDebug("GDCC-MakeLib stdout: {StdOut}", stdout);
        _logger.LogDebug("GDCC-MakeLib stderr: {StdErr}", stderr);
        _logger.LogDebug("Process exit code: {ExitCode}.", process.ExitCode);

        return process.ExitCode;
    }

    private async Task<int> CompileAsync(string gdccCcPath)
    {
        var args = BuildCompileArgs();
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

        _logger.LogDebug("GDCC-CC stdout: {StdOut}", stdout);
        _logger.LogDebug("GDCC-CC stderr: {StdErr}", stderr);
        _logger.LogDebug("Process exit code: {ExitCode}.", process.ExitCode);

        return process.ExitCode;
    }

    private async Task<int> LinkAsync(string gdccLdPath)
    {
        var args = BuildLinkArgs();
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

        _logger.LogDebug("GDCC-LD stdout: {StdOut}", stdout);
        _logger.LogDebug("GDCC-LD stderr: {StdErr}", stderr);
        _logger.LogDebug("Process exit code: {ExitCode}.", process.ExitCode);

        return process.ExitCode;
    }

    private IEnumerable<string> BuildMakeLibArgs()
    {
        yield return $"-co {_makeLibOutputPath}";
        
        if (_task.LinkLibc) yield return "libc";
        if (_task.LinkLibGdcc) yield return "libGDCC";
        
        yield return $"--target-engine {_task.TargetEngine}";
    }

    private IEnumerable<string> BuildCompileArgs()
    {
        yield return $"-co {_compiledOutputPath}";
        yield return _task.InputFilePath!;
        yield return $"--target-engine {_task.TargetEngine}";
    }

    private IEnumerable<string> BuildLinkArgs()
    {
        yield return $"-o {_task.OutputFilePath}";
        
        if (_task.LinkLibc || _task.LinkLibGdcc) yield return _makeLibOutputPath;
        
        yield return _compiledOutputPath;
        yield return $"--target-engine {_task.TargetEngine}";
    }
}
