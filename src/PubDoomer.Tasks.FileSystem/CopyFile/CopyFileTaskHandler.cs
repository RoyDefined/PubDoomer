using Microsoft.Extensions.Logging;
using PubDoomer.Engine.TaskInvokation.Context;
using PubDoomer.Engine.TaskInvokation.Orchestration;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using PubDoomer.Tasks.FileSystem.CopyFile;
using PubDoomer.Tasks.FileSystem.Utils;

public sealed class CopyFileTaskHandler : ITaskHandler
{
    private readonly ILogger _logger;
    private readonly IInvokableTask _taskContext;
    private readonly TaskInvokeContext _invokeContext;

    private readonly ObservableCopyFileTask _task;

    public CopyFileTaskHandler(
        ILogger<CopyFileTaskHandler> logger, IInvokableTask taskContext, TaskInvokeContext invokeContext)
    {
        if (taskContext.Task is not ObservableCopyFileTask task)
        {
            throw new ArgumentException($"The given task is not a {nameof(ObservableCopyFileTask)}.");
        }

        _logger = logger;
        _taskContext = taskContext;
        _invokeContext = invokeContext;

        _task = task;
    }

    public ValueTask<bool> HandleAsync()
    {
        var sourceFilePath = _task.SourceFile;
        var targetFilePath = _task.TargetFile;

        if (string.IsNullOrWhiteSpace(sourceFilePath))
        {
            _taskContext.TaskOutput.Add(TaskOutputResult.CreateError("No source file was specified."));
            return ValueTask.FromResult(false);
        }

        if (string.IsNullOrWhiteSpace(targetFilePath))
        {
            _taskContext.TaskOutput.Add(TaskOutputResult.CreateError("No target file was specified."));
            return ValueTask.FromResult(false);
        }

        _logger.LogDebug("Copying file from {Source} to {Target}.", sourceFilePath, targetFilePath);

        if (!File.Exists(sourceFilePath))
        {
            _taskContext.TaskOutput.Add(TaskOutputResult.CreateError($"The source file '{sourceFilePath}' does not exist."));
            return ValueTask.FromResult(false);
        }

        if (File.Exists(targetFilePath))
        {
            _taskContext.TaskOutput.Add(TaskOutputResult.CreateWarning("The target file already exists. To be safe the task does not proceed."));
            return ValueTask.FromResult(false);
        }

        try
        {
            TransferUtil.TransferFile(new FileInfo(sourceFilePath), targetFilePath, TransferStratergyType.Copy);
            _taskContext.TaskOutput.Add(TaskOutputResult.CreateMessage($"Copied file to '{targetFilePath}' successfully."));
            return ValueTask.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while copying the file.");
            _taskContext.TaskOutput.Add(TaskOutputResult.CreateError($"An error occurred while copying the file: {ex.Message}"));
            return ValueTask.FromResult(false);
        }
    }
}
