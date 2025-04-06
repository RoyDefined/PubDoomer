using Microsoft.Extensions.Logging;
using PubDoomer.Engine.TaskInvokation.Context;
using PubDoomer.Engine.TaskInvokation.Orchestration;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using PubDoomer.Tasks.Compile.Extensions;
using PubDoomer.Tasks.Compile.Utils;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading.Channels;

namespace PubDoomer.Tasks.Compile.Acc;

public sealed class AccCompileTaskHandler : ITaskHandler
{
    private readonly ILogger _logger;
    private readonly IInvokableTask _taskContext;
    private readonly TaskInvokeContext _invokeContext;

    private readonly ObservableAccCompileTask _task;

    private readonly string _errorFilePath;
    private readonly CompositeFormat _errorFileRenamePathTemplate;

    public AccCompileTaskHandler(
        ILogger<AccCompileTaskHandler> logger, IInvokableTask taskContext, TaskInvokeContext invokeContext)
    {
        if (taskContext.Task is not ObservableAccCompileTask task)
        {
            throw new ArgumentException($"The given task is not a {nameof(ObservableAccCompileTask)}.");
        }

        _logger = logger;
        _taskContext = taskContext;
        _invokeContext = invokeContext;

        _task = task;

        _errorFilePath = Path.Combine(Path.GetDirectoryName(task.InputFilePath)!, "acs.err");
        _errorFileRenamePathTemplate = CompositeFormat.Parse(Path.Combine(Path.GetDirectoryName(task.InputFilePath)!, "acs.err.backup{0}"));
    }

    public async ValueTask<bool> HandleAsync()
    {
        var path = _invokeContext.ContextBag.GetAccCompilerExecutableFilePath();
        _logger.LogDebug("Invoking {TaskName}. Input path: {InputFilePath}. Output path: {OutputFilePath}. Location of ACC executable: {AccExecutablePath}", nameof(AccCompileTaskHandler), _task.InputFilePath, _task.OutputFilePath, path);

        // Verify the task has a name.
        if (_task.Name == null)
        {
            _taskContext.TaskOutput.Add(TaskOutputResult.CreateError("Task is missing a name."));
            return false;
        }

        // Check for existing 'acs.err' file.
        // If found, move this file because otherwise the compiler gets rid of it.
        if (File.Exists(_errorFilePath))
        {
            File.Move(_errorFilePath, GetValidBackupPath());
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
            _taskContext.TaskOutput.Add(TaskOutputResult.CreateError("Code execution failed due to an error.", ex));
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
            // Check for `acs.err`. Otherwise give a generic error.
            var compileResult = await HandleAccErrAsync();
            if (compileResult == null)
            {
                _taskContext.TaskOutput.Add(TaskOutputResult.CreateError("Compilation failed for unknown reason."));
                return false;
            }

            _taskContext.TaskOutput.Add(TaskOutputResult.CreateError($"Compilation failed. {compileResult}"));
            return false;
        }

        return true;
    }

    private void HandleStdout(string line)
    {
        // We do not use the stdout.
    }

    private void HandleStdErr(string line)
    {
        // We do not use the stderr.
    }

    private IEnumerable<string> BuildArguments()
    {
        yield return $"\"{_task.InputFilePath}\"";
        yield return $"\"{_task.OutputFilePath}\"";
    }

    private string GetValidBackupPath()
    {
        var path = string.Format(CultureInfo.InvariantCulture, _errorFileRenamePathTemplate, string.Empty);
        var nextFileIndex = 0;
        while (File.Exists(path))
        {
            nextFileIndex++;
            path = string.Format(CultureInfo.InvariantCulture, _errorFileRenamePathTemplate, $" ({nextFileIndex})");
        }

        return path;
    }

    private async ValueTask<string?> HandleAccErrAsync()
    {
        if (!File.Exists(_errorFilePath))
        {
            return null;
        }

        string content;
        using (var reader = new StreamReader(_errorFilePath))
        {
            content = await reader.ReadToEndAsync();
        }

        // If the error file should not be kept then remove the 'acs.err' file.
        if (!_task.KeepAccErrFile)
        {
            _logger.LogDebug("Remove 'acs.err' file as configured.");
            File.Delete(_errorFilePath);
        }

        return content;
    }
}