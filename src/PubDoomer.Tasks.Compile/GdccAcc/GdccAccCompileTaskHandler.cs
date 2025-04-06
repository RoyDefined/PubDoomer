using Microsoft.Extensions.Logging;
using PubDoomer.Engine.TaskInvokation.Context;
using PubDoomer.Engine.TaskInvokation.Orchestration;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using PubDoomer.Engine.TaskInvokation.Utils;
using PubDoomer.Tasks.Compile.Extensions;

namespace PubDoomer.Tasks.Compile.GdccAcc;

public sealed class GdccAccCompileTaskHandler : ITaskHandler
{
    private const string CompileResultWarningPrefix = "WARNING: ";
    private const string CompileResultErrorPrefix = "ERROR: ";

    private readonly ILogger _logger;
    private readonly IInvokableTask _taskContext;
    private readonly TaskInvokeContext _invokeContext;
    private readonly ObservableGdccAccCompileTask _task;

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
        var path = _invokeContext.ContextBag.GetGdccAccCompilerExecutableFilePath();
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
        // TODO: Continue if possible and check if we also have compiler errors, should the process have executed succesfully?
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

    private IEnumerable<string> BuildArguments()
    {
        yield return $"\"{_task.InputFilePath}\"";
        yield return $"-o \"{_task.OutputFilePath}\"";

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
    }

    private static TaskOutputResult? ParseLine(string line)
    {
        return line switch
        {
            { } when line.StartsWith(CompileResultWarningPrefix) => TaskOutputResult.CreateWarning(line[CompileResultWarningPrefix.Length..]),
            { } when line.StartsWith(CompileResultErrorPrefix) => TaskOutputResult.CreateError(line[CompileResultErrorPrefix.Length..]),
            _ => null
        };
    }
}