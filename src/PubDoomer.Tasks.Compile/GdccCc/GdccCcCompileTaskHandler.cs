using Microsoft.Extensions.Logging;
using PubDoomer.Engine.TaskInvokation.Context;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using PubDoomer.Tasks.Compile.Extensions;
using System.Diagnostics;
using PubDoomer.Engine.Abstract;
using SystemProcess = System.Diagnostics.Process;
using PubDoomer.Engine.TaskInvokation.Orchestration;
using PubDoomer.Tasks.Compile.GdccAcc;
using System.Text.RegularExpressions;
using PubDoomer.Engine.TaskInvokation.Utils;
using System.IO;

namespace PubDoomer.Tasks.Compile.GdccCc;

public sealed partial class GdccCcCompileTaskHandler : ITaskHandler
{
    private static readonly string _tempDirectory = Path.Combine(EngineStatics.TemporaryDirectory, "Gdcc-cc");
    private static readonly string _makeLibOutputPath = Path.Combine(_tempDirectory, "makelibfile.ir");
    private static readonly string _compiledOutputPath = Path.Combine(_tempDirectory, "file.ir");

    private readonly ILogger _logger;
    private readonly IInvokableTask _taskContext;
    private readonly TaskInvokeContext _invokeContext;
    private readonly ObservableGdccCcCompileTask _task;

    // Match pattern like: "warning:...path with spaces...:14:18:message here"
    [GeneratedRegex(@"^(warning|error):\s*(.*?):(\d+):(\d+):\s*(.+)$", RegexOptions.IgnoreCase)]
    private static partial Regex StdErrMessageMatcher();

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

        var gdccCcPath = GetGdccCcCompilerExecutableFilePath();
        var gdccMakeLibPath = GetGdccMakeLibCompilerExecutableFilePath();
        var gdccLdPath = GetGdccLdCompilerExecutableFilePath();

        _logger.LogDebug("Invoking {TaskName}. Input path: {InputFilePath}. Output path: {OutputFilePath}.", nameof(GdccCcCompileTaskHandler), _task.InputFilePath, _task.OutputFilePath);

        Directory.CreateDirectory(_tempDirectory);
        
        // Compile libc and/or libGDCC, unless explicitly specified not to.
        // This setting is merged as to call the process once with both parameters.
        if (_task.LinkLibc || _task.LinkLibGdcc)
        {
            _logger.LogDebug("Compiling libc and/or libGDCC. Location of GDCC-MakeLib executable: {GdccMakeLibExecutablePath}. Compile Libc: {CompileLibc}. Compile LibGDCC: {CompileLibGdcc}.", gdccMakeLibPath, _task.LinkLibc, _task.LinkLibGdcc);

            // TODO: Use
            using var makeLibStdOutStream = new MemoryStream();
            using var makeLibStdErrStream = new MemoryStream();

            bool makeLibSucceeded;
            try
            {
                makeLibSucceeded = await TaskHelper.RunProcessAsync(
                    gdccMakeLibPath,
                    BuildMakeLibArgs(),
                    makeLibStdOutStream,
                    makeLibStdErrStream,
                    HandleStdout,
                    HandleStdErr);
            }

            // Premature exception was thrown, not related to the makelib operation.
            catch (Exception ex)
            {
                _taskContext.TaskOutput.Add(TaskOutputResult.CreateError("MakeLib process failed due to an error", ex));
                return false;
            }

            // Makelib returned an error.
            if (!makeLibSucceeded)
            {
                _taskContext.TaskOutput.Add(TaskOutputResult.CreateError("MakeLib process failed"));
                return false;
            }

            _taskContext.TaskOutput.Add(TaskOutputResult.CreateMessage("Makelib operation succeeded"));
        }

        // Compile the input files.
        _logger.LogDebug("Compiling. Location of GDCC-CC executable: {GdccCcExecutablePath}.", gdccCcPath);

        // TODO: Use
        using var compilationStdOutStream = new MemoryStream();
        using var compilationStdErrStream = new MemoryStream();

        bool compilationSucceeded;
        try
        {
            compilationSucceeded = await TaskHelper.RunProcessAsync(
                gdccCcPath,
                BuildCompileArgs(),
                compilationStdOutStream,
                compilationStdErrStream,
                HandleStdout,
                HandleStdErr);
        }

        // Premature exception was thrown, not related to the compilation.
        catch (Exception ex)
        {
            _taskContext.TaskOutput.Add(TaskOutputResult.CreateError("Compilation failed due to an error", ex));
            return false;
        }

        // Compilation returned an error.
        if (!compilationSucceeded)
        {
            _taskContext.TaskOutput.Add(TaskOutputResult.CreateError("Compilation failed"));
            return false;
        }

        _taskContext.TaskOutput.Add(TaskOutputResult.CreateMessage("Compilation succeeded"));

        // Link the files.
        _logger.LogDebug("Linking. Location of GDCC-LD executable: {GdccLdExecutablePath}", gdccLdPath);

        // TODO: Use
        using var linkingStdOutStream = new MemoryStream();
        using var linkingStdErrStream = new MemoryStream();

        bool linkingSucceeded;
        try
        {
            linkingSucceeded = await TaskHelper.RunProcessAsync(
                gdccLdPath,
                BuildLinkArgs(),
                linkingStdOutStream,
                linkingStdErrStream,
                HandleStdout,
                HandleStdErr);
        }

        // Premature exception was thrown, not related to the linking.
        catch (Exception ex)
        {
            _taskContext.TaskOutput.Add(TaskOutputResult.CreateError("Linking failed due to an error", ex));
            return false;
        }

        // Linking returned an error.
        if (!linkingSucceeded)
        {
            _taskContext.TaskOutput.Add(TaskOutputResult.CreateError("Linking failed"));
            return false;
        }

        _taskContext.TaskOutput.Add(TaskOutputResult.CreateMessage("Linking succeeded"));
        return true;
    }
    
    private string GetGdccCcCompilerExecutableFilePath()
    {
        var path = _invokeContext.ContextBag.GetGdccCcCompilerExecutableFilePath();
        
        // Handle relative path
        if (!Path.IsPathRooted(path))
        {
            if (string.IsNullOrWhiteSpace(_invokeContext.ProjectDirectory))
            {
                throw new ArgumentException($"Failed to update relative GDCC-CC compiler executable path ({path}). No working directory was specified. Either the working directory must be specified or the GDCC-CC compiler executable path must be absolute.");
            }
            
            path = Path.Combine(_invokeContext.ProjectDirectory, path);
        }
        
        return path;
    }
    
    private string GetGdccMakeLibCompilerExecutableFilePath()
    {
        var path = _invokeContext.ContextBag.GetGdccMakeLibCompilerExecutableFilePath();
        
        // Handle relative path
        if (!Path.IsPathRooted(path))
        {
            if (string.IsNullOrWhiteSpace(_invokeContext.ProjectDirectory))
            {
                throw new ArgumentException($"Failed to update relative GDCC-MakeLib compiler executable path ({path}). No working directory was specified. Either the working directory must be specified or the GDCC-MakeLib compiler executable path must be absolute.");
            }
            
            path = Path.Combine(_invokeContext.ProjectDirectory, path);
        }
        
        return path;
    }
    
    private string GetGdccLdCompilerExecutableFilePath()
    {
        var path = _invokeContext.ContextBag.GetGdccLdCompilerExecutableFilePath();
        
        // Handle relative path
        if (!Path.IsPathRooted(path))
        {
            if (string.IsNullOrWhiteSpace(_invokeContext.ProjectDirectory))
            {
                throw new ArgumentException($"Failed to update relative GDCC-Ld compiler executable path ({path}). No working directory was specified. Either the working directory must be specified or the GDCC-Ld compiler executable path must be absolute.");
            }
            
            path = Path.Combine(_invokeContext.ProjectDirectory, path);
        }
        
        return path;
    }

    private IEnumerable<string> BuildMakeLibArgs()
    {
        yield return $"-co {_makeLibOutputPath}";
        
        if (_task.LinkLibc) yield return "libc";
        if (_task.LinkLibGdcc) yield return "libGDCC";
        
        yield return $"--target-engine {_task.TargetEngine}";

        if (_task.DontWarnForwardReferences)
        {
            yield return "--no-warn-forward-reference";
        }
    }

    private IEnumerable<string> BuildCompileArgs()
    {
        foreach (var directory in _task.IncludeDirectories)
        {
            var directoryPath = directory.Value;
            ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath, $"{nameof(_task.IncludeDirectories)}.{nameof(_task.InputFilePath)}");
        
            // Handle relative input path
            if (!Path.IsPathRooted(directoryPath))
            {
                if (string.IsNullOrWhiteSpace(_invokeContext.WorkingDirectory))
                {
                    throw new ArgumentException($"Failed to update relative input path for included directory ({directoryPath}). No working directory was specified. Either the working directory must be specified or the path to the included directory must be absolute.");
                }
            
                directoryPath = Path.Combine(_invokeContext.WorkingDirectory, directoryPath);
            }
            
            yield return $"-i \"{directoryPath}\"";
        }
        
        foreach (var macro in _task.Macros)
        {
            yield return $"-D \"{macro.Value}\"";
        }
        
        var inputPath = _task.InputFilePath;
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath, nameof(_task.InputFilePath));
        
        // Handle relative input path
        if (!Path.IsPathRooted(inputPath))
        {
            if (string.IsNullOrWhiteSpace(_invokeContext.WorkingDirectory))
            {
                throw new ArgumentException($"Failed to update relative input path for ACS file input ({inputPath}). No working directory was specified. Either the working directory must be specified or the path to the ACS file input must be absolute.");
            }
            
            inputPath = Path.Combine(_invokeContext.WorkingDirectory, inputPath);
        }
        
        yield return $"-co {_compiledOutputPath}";
        yield return inputPath;
        yield return $"--target-engine {_task.TargetEngine}";
    }

    private IEnumerable<string> BuildLinkArgs()
    {
        var outputPath = _task.OutputFilePath;
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath, nameof(_task.OutputFilePath));
        
        // Handle relative input path
        if (!Path.IsPathRooted(outputPath))
        {
            if (string.IsNullOrWhiteSpace(_invokeContext.WorkingDirectory))
            {
                throw new ArgumentException($"Failed to update relative input path for ACS file output ({outputPath}). No working directory was specified. Either the working directory must be specified or the path to the ACS file output must be absolute.");
            }
            
            outputPath = Path.Combine(_invokeContext.WorkingDirectory, outputPath);
        }
        
        yield return $"-o {outputPath}";
        
        if (_task.LinkLibc || _task.LinkLibGdcc) yield return _makeLibOutputPath;
        
        yield return _compiledOutputPath;
        yield return $"--target-engine {_task.TargetEngine}";
    }

    private void HandleStdout(string line)
    {
        _taskContext.TaskOutput.Add(TaskOutputResult.CreateMessage(line));
    }

    private void HandleStdErr(string line)
    {
        var result = ParseLine(line);
        if (result == null)
        {
            _logger.LogWarning("Encountered an invalid line in compile results: '{Line}'", line);
            return;
        }
        _taskContext.TaskOutput.Add(result);
    }

    private TaskOutputResult? ParseLine(string line)
    {
        // Using regex we determine the formatting of the message.
        // Notably it always contains 'warning: ' or 'error: '

        if (string.IsNullOrWhiteSpace(line))
            return null;

        var match = StdErrMessageMatcher().Match(line);
        if (!match.Success)
            return null;

        var type = match.Groups[1].Value.ToLowerInvariant();
        var file = match.Groups[2].Value;
        var faulthyLine = match.Groups[3].Value;
        var faulthyCharacter = match.Groups[4].Value;
        var message = match.Groups[5].Value.Trim();

        // Get the relative path to the file compared to the source and build a new message from that.
        var fileDirectory = Path.GetDirectoryName(_task.InputFilePath!)!;
        var relativeFilePath = Path.GetRelativePath(fileDirectory, file);
        message = $"File \"{relativeFilePath}\", line {faulthyLine}, char {faulthyCharacter}: {message}";

        return type switch
        {
            "warning" => TaskOutputResult.CreateWarning(message),
            "error" => TaskOutputResult.CreateError(message),
            _ => null
        };
    }
}
