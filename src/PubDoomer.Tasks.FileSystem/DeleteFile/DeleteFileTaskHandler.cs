using Microsoft.Extensions.Logging;
using PubDoomer.Engine.TaskInvokation.Context;
using PubDoomer.Engine.TaskInvokation.Orchestration;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using PubDoomer.Tasks.FileSystem.Utils;

namespace PubDoomer.Tasks.FileSystem.DeleteFile;

public sealed class DeleteFileTaskHandler : ITaskHandler
{
    private readonly ILogger _logger;
    private readonly IInvokableTask _taskContext;
    private readonly TaskInvokeContext _invokeContext;

    private readonly ObservableDeleteFileTask _task;

    public DeleteFileTaskHandler(
        ILogger<DeleteFileTaskHandler> logger, IInvokableTask taskContext, TaskInvokeContext invokeContext)
    {
        if (taskContext.Task is not ObservableDeleteFileTask task)
        {
            throw new ArgumentException($"The given task is not a {nameof(ObservableDeleteFileTask)}.");
        }

        _logger = logger;
        _taskContext = taskContext;
        _invokeContext = invokeContext;

        _task = task;
    }

    public ValueTask<bool> HandleAsync()
    {
        var targetFilePath = GetArgument();
        
        _logger.LogDebug("Deleting file at {Target}", targetFilePath);

        if (!File.Exists(targetFilePath))
        {
            _taskContext.TaskOutput.Add(TaskOutputResult.CreateMessage($"The target file '{targetFilePath}' does not exist. SKipping task."));
            return ValueTask.FromResult(true);
        }

        try
        {
            File.Delete(targetFilePath);
            _taskContext.TaskOutput.Add(TaskOutputResult.CreateMessage($"Deleted file '{targetFilePath}' successfully."));
            return ValueTask.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting the file.");
            _taskContext.TaskOutput.Add(TaskOutputResult.CreateError($"An error occurred while deleting the file: {ex.Message}"));
            return ValueTask.FromResult(false);
        }
    }
    
    private string GetArgument()
    {
        var targetFilePath = _task.TargetFilePath;
        ArgumentException.ThrowIfNullOrWhiteSpace(targetFilePath, nameof(_task.TargetFilePath));
        
        // Handle relative output path
        if (!Path.IsPathRooted(targetFilePath))
        {
            if (string.IsNullOrWhiteSpace(_invokeContext.WorkingDirectory))
            {
                throw new ArgumentException($"Failed to update relative path for file target ({targetFilePath}). No working directory was specified. Either the working directory must be specified or the path to the target file input must be absolute.");
            }
            
            targetFilePath = Path.Combine(_invokeContext.WorkingDirectory, targetFilePath);
        }
        
        return targetFilePath;
    }
}