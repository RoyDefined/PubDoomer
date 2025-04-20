using Microsoft.Extensions.Logging;
using PubDoomer.Engine.TaskInvokation.Context;
using PubDoomer.Engine.TaskInvokation.Orchestration;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using PubDoomer.Engine.TaskInvokation.Utils;
using PubDoomer.Tasks.Compile.Extensions;
using System.Text.RegularExpressions;

namespace PubDoomer.Tasks.Compile.GdccAcc;

public sealed partial class GdccAccCompileTaskHandler : ITaskHandler
{
    private readonly ILogger _logger;
    private readonly IInvokableTask _taskContext;
    private readonly TaskInvokeContext _invokeContext;
    private readonly ObservableGdccAccCompileTask _task;

    // Match pattern like: "warning:...path with spaces...:14:18:message here"
    [GeneratedRegex(@"^(warning|error):\s*(.*?):(\d+):(\d+):\s*(.+)$", RegexOptions.IgnoreCase)]
    private static partial Regex StdErrMessageMatcher();

    public GdccAccCompileTaskHandler(
        ILogger<GdccAccCompileTaskHandler> logger, IInvokableTask taskContext, TaskInvokeContext invokeContext)
    {
        if (taskContext.Task is not ObservableGdccAccCompileTask task)
        {
            throw new ArgumentException($"The given task is not a {nameof(ObservableGdccAccCompileTask)}.");
        }

        _logger = logger;
        _taskContext = taskContext;
        _invokeContext = invokeContext;
        _task = task;
    }

    public async ValueTask<bool> HandleAsync()
    {
        var path = GetGdccAccCompilerExecutableFilePath();
        _logger.LogDebug("Invoking {TaskName}. Input path: {InputFilePath}. Output path: {OutputFilePath}. Location of GDCC-ACC executable: {GdccAccExecutablePath}", nameof(GdccAccCompileTaskHandler), _task.InputFilePath, _task.OutputFilePath, path);

        // Verify the task has a name.
        if (_task.Name == null)
        {
            _taskContext.TaskOutput.Add(TaskOutputResult.CreateError("Task is missing a name."));
            return false;
        }

        using var stdOutStream = new MemoryStream();
        using var stdErrStream = new MemoryStream();

        bool succeeded;
        try
        {

            succeeded = await TaskHelper.RunProcessAsync(
                path,
                BuildArguments(),
                stdOutStream,
                stdErrStream,
                HandleStdout,
                HandleStdErr);
        }

        // Premature exception was thrown, not related to compilation.
        catch (Exception ex)
        {
            _taskContext.TaskOutput.Add(TaskOutputResult.CreateError("Code execution failed due to an error", ex));
            return false;
        }

        // Write stdout and stderr if configured.
        if (_task.GenerateStdOutAndStdErrFiles)
        {
            await TaskHelper.WriteToFileAsync(stdOutStream, _task.Name, "stdout.txt");
            await TaskHelper.WriteToFileAsync(stdErrStream, _task.Name, "stderr.txt");
        }

        // The compiler returned an error.
        if (!succeeded)
        {
            _taskContext.TaskOutput.Add(TaskOutputResult.CreateError("Compilation failed"));
            return false;
        }

        return true;
    }
    
    private string GetGdccAccCompilerExecutableFilePath()
    {
        var path = _invokeContext.ContextBag.GetGdccAccCompilerExecutableFilePath();
        
        // Handle relative path
        if (!Path.IsPathRooted(path))
        {
            if (string.IsNullOrWhiteSpace(_invokeContext.WorkingDirectory))
            {
                throw new ArgumentException($"Failed to update relative GDCC compiler executable path ({path}). No working directory was specified. Either the working directory must be specified or the GDCC compiler executable path must be absolute.");
            }
            
            path = Path.Combine(_invokeContext.WorkingDirectory, path);
        }
        
        return path;
    }

    private IEnumerable<string> BuildArguments()
    {
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
        
        yield return $"\"{inputPath}\"";
        yield return $"-o \"{outputPath}\"";

        if (_task.DontWarnForwardReferences)
        {
            yield return "--no-warn-forward-reference";
        }
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