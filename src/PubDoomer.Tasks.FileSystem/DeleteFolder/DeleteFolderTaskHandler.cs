using Microsoft.Extensions.Logging;
using PubDoomer.Engine.TaskInvokation.Context;
using PubDoomer.Engine.TaskInvokation.Orchestration;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using PubDoomer.Tasks.FileSystem.Utils;

namespace PubDoomer.Tasks.FileSystem.DeleteFolder;

public sealed class DeleteFolderTaskHandler : ITaskHandler
{
    private readonly ILogger _logger;
    private readonly IInvokableTask _taskContext;
    private readonly TaskInvokeContext _invokeContext;

    private readonly ObservableDeleteFolderTask _task;

    public DeleteFolderTaskHandler(
        ILogger<DeleteFolderTaskHandler> logger, IInvokableTask taskContext, TaskInvokeContext invokeContext)
    {
        if (taskContext.Task is not ObservableDeleteFolderTask task)
        {
            throw new ArgumentException($"The given task is not a {nameof(ObservableDeleteFolderTask)}.");
        }

        _logger = logger;
        _taskContext = taskContext;
        _invokeContext = invokeContext;

        _task = task;
    }

    public ValueTask<bool> HandleAsync()
    {
        var targetFolderPath = GetArgument();
        
        _logger.LogDebug("Deleting folder at {Target}", targetFolderPath);

        if (!Directory.Exists(targetFolderPath))
        {
            _taskContext.TaskOutput.Add(TaskOutputResult.CreateMessage($"The target folder '{targetFolderPath}' does not exist. SKipping task."));
            return ValueTask.FromResult(true);
        }

        try
        {
            Directory.Delete(targetFolderPath, true);
            _taskContext.TaskOutput.Add(TaskOutputResult.CreateMessage($"Deleted folder '{targetFolderPath}' successfully."));
            return ValueTask.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting the folder.");
            _taskContext.TaskOutput.Add(TaskOutputResult.CreateError($"An error occurred while deleting the folder: {ex.Message}"));
            return ValueTask.FromResult(false);
        }
    }
    
    private string GetArgument()
    {
        var targetPath = _task.TargetFolder;
        ArgumentException.ThrowIfNullOrWhiteSpace(targetPath, nameof(_task.TargetFolder));
        
        // Handle relative output path
        if (!Path.IsPathRooted(targetPath))
        {
            if (string.IsNullOrWhiteSpace(_invokeContext.WorkingDirectory))
            {
                throw new ArgumentException($"Failed to update relative path for folder target ({targetPath}). No working directory was specified. Either the working directory must be specified or the path to the target folder input must be absolute.");
            }
            
            targetPath = Path.Combine(_invokeContext.WorkingDirectory, targetPath);
        }
        
        return targetPath;
    }
}