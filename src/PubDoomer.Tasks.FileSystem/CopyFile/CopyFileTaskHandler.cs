using Microsoft.Extensions.Logging;
using PubDoomer.Engine.TaskInvokation.Context;
using PubDoomer.Engine.TaskInvokation.Orchestration;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using PubDoomer.Tasks.FileSystem.Utils;

namespace PubDoomer.Tasks.FileSystem.CopyFile;

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
        var (sourceFilePath, targetFilePath) = GetArguments();
        _logger.LogDebug("Copying file from {Source} to {Target}.", sourceFilePath, targetFilePath);

        if (!File.Exists(sourceFilePath))
        {
            _taskContext.TaskOutput.Add(TaskOutputResult.CreateError($"The source file '{sourceFilePath}' does not exist."));
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
    
    private (string sourceFilePath, string targetFilePath) GetArguments()
    {
        var sourcePath = _task.SourceFile;
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath, nameof(_task.SourceFile));
        
        // Handle relative input path
        if (!Path.IsPathRooted(sourcePath))
        {
            if (string.IsNullOrWhiteSpace(_invokeContext.WorkingDirectory))
            {
                throw new ArgumentException($"Failed to update relative path for file source ({sourcePath}). No working directory was specified. Either the working directory must be specified or the path to the source file input must be absolute.");
            }
            
            sourcePath = Path.Combine(_invokeContext.WorkingDirectory, sourcePath);
        }
        
        var targetPath = _task.TargetFile;
        ArgumentException.ThrowIfNullOrWhiteSpace(targetPath, nameof(_task.TargetFile));
        
        // Handle relative output path
        if (!Path.IsPathRooted(targetPath))
        {
            if (string.IsNullOrWhiteSpace(_invokeContext.WorkingDirectory))
            {
                throw new ArgumentException($"Failed to update relative path for file target ({targetPath}). No working directory was specified. Either the working directory must be specified or the path to the target file input must be absolute.");
            }
            
            targetPath = Path.Combine(_invokeContext.WorkingDirectory, targetPath);
        }
        
        return (sourcePath, targetPath);
    }
}