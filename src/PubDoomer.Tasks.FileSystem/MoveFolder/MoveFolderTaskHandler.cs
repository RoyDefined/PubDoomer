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
        var (sourceFolderPath, targetFolderPath) = GetArguments();
        
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
    
    private (string sourceFolderPath, string targetFolderPath) GetArguments()
    {
        var sourcePath = _task.SourceFolder;
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath, nameof(_task.SourceFolder));
        
        // Handle relative input path
        if (!Path.IsPathRooted(sourcePath))
        {
            if (string.IsNullOrWhiteSpace(_invokeContext.WorkingDirectory))
            {
                throw new ArgumentException($"Failed to update relative path for folder source ({sourcePath}). No working directory was specified. Either the working directory must be specified or the path to the source folder input must be absolute.");
            }
            
            sourcePath = Path.Combine(_invokeContext.WorkingDirectory, sourcePath);
        }
        
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
        
        return (sourcePath, targetPath);
    }
}
