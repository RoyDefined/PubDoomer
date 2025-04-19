using System.IO.Compression;
using Microsoft.Extensions.Logging;
using PubDoomer.Engine.TaskInvokation.Context;
using PubDoomer.Engine.TaskInvokation.Orchestration;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using PubDoomer.Tasks.FileSystem.Utils;

namespace PubDoomer.Tasks.FileSystem.ZipFolder;

public sealed class ZipFolderTaskHandler : ITaskHandler
{
    private readonly ILogger _logger;
    private readonly IInvokableTask _taskContext;
    private readonly TaskInvokeContext _invokeContext;

    private readonly ObservableZipFolderTask _task;

    public ZipFolderTaskHandler(
        ILogger<ZipFolderTaskHandler> logger, IInvokableTask taskContext, TaskInvokeContext invokeContext)
    {
        if (taskContext.Task is not ObservableZipFolderTask task)
        {
            throw new ArgumentException($"The given task is not a {nameof(ObservableZipFolderTask)}.");
        }

        _logger = logger;
        _taskContext = taskContext;
        _invokeContext = invokeContext;

        _task = task;
    }

    public ValueTask<bool> HandleAsync()
    {
        var (sourceFolderPath, targetFilePath) = GetArguments();
    
        _logger.LogDebug("Zipping folder from {Source} to {Target}.",
            sourceFolderPath, targetFilePath);

        if (!Directory.Exists(sourceFolderPath))
        {
            _taskContext.TaskOutput.Add(TaskOutputResult.CreateError(
                $"The source folder '{sourceFolderPath}' does not exist."));
            return ValueTask.FromResult(false);
        }

        try
        {
            // Ensure the target directory exists
            var targetDirectory = Path.GetDirectoryName(targetFilePath);
            if (!string.IsNullOrEmpty(targetDirectory) && !Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            // If the target zip file already exists, delete it.
            // TODO: Configurable.
            if (File.Exists(targetFilePath))
            {
                File.Delete(targetFilePath);
            }

            // Zip the folder
            ZipFile.CreateFromDirectory(sourceFolderPath, targetFilePath, CompressionLevel.Optimal, includeBaseDirectory: false);

            _logger.LogInformation("Zipped folder {Source} to file {Target} successfully.",
                sourceFolderPath, targetFilePath);

            _taskContext.TaskOutput.Add(TaskOutputResult.CreateMessage(
                $"Zipped folder to '{targetFilePath}' successfully."));
            return ValueTask.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while zipping the folder.");
            _taskContext.TaskOutput.Add(TaskOutputResult.CreateError(
                $"An error occurred while zipping the folder: {ex.Message}"));
            return ValueTask.FromResult(false);
        }
    }
    
    private (string sourceFolderPath, string targetFilePath) GetArguments()
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
        
        var targetFilePath = _task.TargetFilePath;
        ArgumentException.ThrowIfNullOrWhiteSpace(targetFilePath, nameof(_task.TargetFilePath));
        
        // Handle relative output path
        if (!Path.IsPathRooted(targetFilePath))
        {
            if (string.IsNullOrWhiteSpace(_invokeContext.WorkingDirectory))
            {
                throw new ArgumentException($"Failed to update relative path for file path target ({targetFilePath}). No working directory was specified. Either the working directory must be specified or the path to the target file path input must be absolute.");
            }
            
            targetFilePath = Path.Combine(_invokeContext.WorkingDirectory, targetFilePath);
        }
        
        return (sourcePath, targetFilePath);
    }
}