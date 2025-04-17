using Microsoft.Extensions.Logging;
using PubDoomer.Engine.TaskInvokation.Context;
using PubDoomer.Engine.TaskInvokation.Orchestration;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using PubDoomer.Tasks.FileSystem.MoveFolder;
using PubDoomer.Tasks.FileSystem.Utils;

public sealed class MoveFolderTaskHandler : ITaskHandler
{
    private readonly ILogger _logger;
    private readonly IInvokableTask _taskContext;
    private readonly TaskInvokeContext _invokeContext;

    private readonly ObservableMoveFolderTask _task;

    public MoveFolderTaskHandler(
        ILogger<MoveFolderTaskHandler> logger, IInvokableTask taskContext, TaskInvokeContext invokeContext)
    {
        if (taskContext.Task is not ObservableMoveFolderTask task)
        {
            throw new ArgumentException($"The given task is not a {nameof(ObservableMoveFolderTask)}.");
        }

        _logger = logger;
        _taskContext = taskContext;
        _invokeContext = invokeContext;

        _task = task;
    }

    public ValueTask<bool> HandleAsync()
    {
        var sourceFolderPath = _task.SourceFolder;
        var targetFolderPath = _task.TargetFolder;

        if (string.IsNullOrWhiteSpace(sourceFolderPath))
        {
            _taskContext.TaskOutput.Add(TaskOutputResult.CreateError("No source folder was specified."));
            return ValueTask.FromResult(false);
        }

        if (string.IsNullOrWhiteSpace(targetFolderPath))
        {
            _taskContext.TaskOutput.Add(TaskOutputResult.CreateError("No target folder was specified."));
            return ValueTask.FromResult(false);
        }

        _logger.LogDebug("Moving folder from {Source} to {Target}. Recursive: {Recursive}",
            sourceFolderPath, targetFolderPath, _task.Recursive);

        if (!Directory.Exists(sourceFolderPath))
        {
            _taskContext.TaskOutput.Add(TaskOutputResult.CreateError($"The source folder '{sourceFolderPath}' does not exist."));
            return ValueTask.FromResult(false);
        }

        if (Directory.Exists(targetFolderPath))
        {
            _taskContext.TaskOutput.Add(TaskOutputResult.CreateWarning("The target folder already exists. To be safe the task does not proceed."));
            return ValueTask.FromResult(false);
        }

        try
        {
            TransferUtil.TransferDirectory(sourceFolderPath, targetFolderPath, TransferStratergyType.Move, _task.Recursive);

            _taskContext.TaskOutput.Add(TaskOutputResult.CreateMessage($"Moved folder to '{targetFolderPath}' successfully."));
            return ValueTask.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while moving the folder.");
            _taskContext.TaskOutput.Add(TaskOutputResult.CreateError($"An error occurred while moving the folder: {ex.Message}"));
            return ValueTask.FromResult(false);
        }
    }
}
