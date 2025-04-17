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

    public ValueTask<bool> HandleAsync()
    {
        // Does the project already point to the target path?
        // If so, end early.
        var sourceFolderPath = _invokeContext.WorkingDirectory;
        if (string.IsNullOrWhiteSpace(sourceFolderPath))
        {
            _taskContext.TaskOutput.Add(TaskOutputResult.CreateError("No source path provided to copy the project. This probably means there is no project open."));
            return ValueTask.FromResult(false);
        }

        // Determine target folder.
        // If none were specified, the boolean must be checked to generate a temporary directory.
        // If both were specified we will warn and make a temporary folder.
        var targetFolderPath = _task.TargetFolder;
        if (_task.UseTempFolder || string.IsNullOrWhiteSpace(targetFolderPath))
        {
            if (!_task.UseTempFolder)
            {
                _taskContext.TaskOutput.Add(TaskOutputResult.CreateError("No target path provided to copy the project."));
                return ValueTask.FromResult(false);
            }

            if (!string.IsNullOrWhiteSpace(targetFolderPath))
            {
                _taskContext.TaskOutput.Add(TaskOutputResult.CreateWarning("The task is configured to write to a specified target folder and also generate a temporary folder. Picking generation of a temporary folder."));
            }

            targetFolderPath = Path.Combine(EngineStatics.TemporaryDirectory, "CopiedProjects", Path.GetRandomFileName());
        }
        
        _logger.LogDebug("Using target folder: {TargetFolder}", targetFolderPath);

        if (Path.GetFullPath(sourceFolderPath).TrimEnd(Path.DirectorySeparatorChar)
            .Equals(Path.GetFullPath(targetFolderPath).TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
        {
            _taskContext.TaskOutput.Add(TaskOutputResult.CreateMessage("Source and target paths are the same. Skipping copy."));
            return ValueTask.FromResult(true);
        }

        // Copy over the project to the new location.
        // To be safe, fail if the folder exists.
        if (Directory.Exists(targetFolderPath))
        {
            _taskContext.TaskOutput.Add(TaskOutputResult.CreateWarning("The target folder already exists. To be safe the task does not proceed."));
            return ValueTask.FromResult(false);
        }

        // Update the current location of the project.
        TransferUtil.TransferDirectory(sourceFolderPath, targetFolderPath, TransferStratergyType.Copy, true);
        _invokeContext.WorkingDirectory = targetFolderPath;

        _taskContext.TaskOutput.Add(TaskOutputResult.CreateMessage($"Updated the working directory to {targetFolderPath}"));
        return ValueTask.FromResult(true);
    }
}