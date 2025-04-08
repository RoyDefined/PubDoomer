using Microsoft.Extensions.Logging;
using PubDoomer.Engine.TaskInvokation.Context;
using PubDoomer.Engine.TaskInvokation.Orchestration;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using PubDoomer.Engine.TaskInvokation.Utils;
using PubDoomer.Tasks.Compile.Extensions;
using System.Text.RegularExpressions;

namespace PubDoomer.Tasks.Compile.Bcc;

public sealed partial class BccCompileTaskHandler : ITaskHandler
{
    private readonly ILogger _logger;
    private readonly IInvokableTask _taskContext;
    private readonly TaskInvokeContext _invokeContext;
    private readonly ObservableBccCompileTask _task;

    // Match pattern like: "...path with spaces...:14:18: warning: message here"
    [GeneratedRegex(@"^(.*?):(\d+):(\d+):\s*(warning|error):\s*(.+)$", RegexOptions.IgnoreCase)]
    private static partial Regex StdErrMessageMatcher();

    public BccCompileTaskHandler(
        ILogger<BccCompileTaskHandler> logger, IInvokableTask taskContext, TaskInvokeContext invokeContext)
    {
        if (taskContext.Task is not ObservableBccCompileTask task)
        {
            throw new ArgumentException($"The given task is not a {nameof(ObservableBccCompileTask)}.");
        }

        _logger = logger;
        _taskContext = taskContext;
        _invokeContext = invokeContext;
        _task = task;
    }

    public async ValueTask<bool> HandleAsync()
    {
        var path = _invokeContext.ContextBag.GetBccCompilerExecutableFilePath();
        _logger.LogDebug("Invoking {TaskName}. Input path: {InputFilePath}. Output path: {OutputFilePath}. Location of BCC executable: {BccExecutablePath}", nameof(BccCompileTaskHandler), _task.InputFilePath, _task.OutputFilePath, path);

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
            _taskContext.TaskOutput.Add(TaskOutputResult.CreateError("Code execution failed due to an error.", ex));
            return false;
        }

        // Write stdout and stderr if configured.
        if (_task.GenerateStdOutAndStdErrFiles)
        {
            await TaskHelper.WriteToFileAsync(stdOutStream, _task.Name, "stdout.txt");
            await TaskHelper.WriteToFileAsync(stdErrStream, _task.Name, "stderr.txt");
        }

        // The compiler failed.
        if (!succeeded)
        {
            _taskContext.TaskOutput.Add(TaskOutputResult.CreateError("Compilation failed."));
            return false;
        }

        return true;
    }

    private IEnumerable<string> BuildArguments()
    {
        foreach (var directory in _task.IncludeDirectories)
        {
            yield return $"-i \"{directory.Value}\"";
        }
        
        yield return $"\"{_task.InputFilePath}\"";
        yield return $"\"{_task.OutputFilePath}\"";
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

        var file = match.Groups[1].Value;
        var faulthyLine = match.Groups[2].Value;
        var faulthyCharacter = match.Groups[3].Value;
        var type = match.Groups[4].Value.ToLowerInvariant();
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