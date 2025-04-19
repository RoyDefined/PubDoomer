using Microsoft.Extensions.Logging;
using PubDoomer.Engine.TaskInvokation.Context;
using PubDoomer.Engine.TaskInvokation.Orchestration;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using PubDoomer.Engine.TaskInvokation.Utils;
using PubDoomer.Tasks.FileSystem.Utils;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading.Channels;
using PubDoomer.Engine.Abstract;

namespace PubDoomer.Tasks.FileSystem.CopyProject;

public sealed class CopyProjectTaskHandler : ITaskHandler
{
    private readonly ILogger _logger;
    private readonly IInvokableTask _taskContext;
    private readonly TaskInvokeContext _invokeContext;

    private readonly ObservableCopyProjectTask _task;

    public CopyProjectTaskHandler(
        ILogger<CopyProjectTaskHandler> logger, IInvokableTask taskContext, TaskInvokeContext invokeContext)
    {
        if (taskContext.Task is not ObservableCopyProjectTask task)
        {
            throw new ArgumentException($"The given task is not a {nameof(ObservableCopyProjectTask)}.");
        }

        _logger = logger;
        _taskContext = taskContext;
        _invokeContext = invokeContext;

        _task = task;
    }

    public async ValueTask<bool> HandleAsync()
    {
        var sourceFolderPath = _invokeContext.WorkingDirectory;
        if (string.IsNullOrWhiteSpace(sourceFolderPath))
        {
            throw new ArgumentException($"Failed to determine working directory because no working directory was specified. This likely means the project was not loaded.");
        }
        
        var targetFolderPath = GetArguments();
        _logger.LogDebug("Using target folder: {TargetFolder}", targetFolderPath);

        if (sourceFolderPath.Equals(targetFolderPath, StringComparison.OrdinalIgnoreCase))
        {
            _taskContext.TaskOutput.Add(TaskOutputResult.CreateMessage("Source and target paths are the same. Skipping copy."));
            return true;
        }

        // Copy over the project to the new location.
        // To be safe, fail if the folder exists.
        if (Directory.Exists(targetFolderPath))
        {
            _taskContext.TaskOutput.Add(TaskOutputResult.CreateWarning("The target folder already exists. To be safe the task does not proceed."));
            return false;
        }

        _taskContext.TaskOutput.Add(TaskOutputResult.CreateMessage("Copying project..."));
        
        // Update the current location of the project.
        await Task.Run(() =>
            TransferUtil.TransferDirectory(sourceFolderPath, targetFolderPath, TransferStratergyType.Copy, true));
        _invokeContext.WorkingDirectory = targetFolderPath;

        _taskContext.TaskOutput.Add(TaskOutputResult.CreateMessage($"Updated the working directory to {targetFolderPath}"));
        return true;
    }
    
    private string GetArguments()
    {
        // Determine target folder.
        // If no target folder was specified, the boolean must be checked to generate a temporary directory.
        // If both were specified we will warn and prioritize making a temporary folder.
        var targetPath = _task.TargetFolder;
        if (_task.UseTempFolder || string.IsNullOrWhiteSpace(targetPath))
        {
            if (!_task.UseTempFolder)
            {
                throw new ArgumentException("No target path provided to copy the project.");
            }

            // We specified to generate a temporary folder, but also specified a target path.
            if (!string.IsNullOrWhiteSpace(targetPath))
            {
                _taskContext.TaskOutput.Add(TaskOutputResult.CreateWarning("The task is configured to write to a specified target folder and also generate a temporary folder. Picking generation of a temporary folder."));
            }

            targetPath = Path.Combine(EngineStatics.TemporaryDirectory, "CopiedProjects", Path.GetRandomFileName());
        }

        else
        {
            // Handle relative output path
            if (!Path.IsPathRooted(targetPath))
            {
                if (string.IsNullOrWhiteSpace(_invokeContext.WorkingDirectory))
                {
                    throw new ArgumentException($"Failed to update relative path for folder target ({targetPath}). No working directory was specified. Either the working directory must be specified or the path to the target folder input must be absolute.");
                }

                targetPath = Path.Combine(_invokeContext.WorkingDirectory, targetPath);
            }
        }
        
        return targetPath;
    }
}