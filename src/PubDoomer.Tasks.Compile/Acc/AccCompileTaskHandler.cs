using Microsoft.Extensions.Logging;
using PubDoomer.Engine.TaskInvokation.Context;
using PubDoomer.Engine.TaskInvokation.Orchestration;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using PubDoomer.Engine.TaskInvokation.Utils;
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
    }

    public async ValueTask<bool> HandleAsync()
    {
        var path = GetAccCompilerExecutableFilePath();
        _logger.LogDebug("Invoking {TaskName}. Input path: {InputFilePath}. Output path: {OutputFilePath}. Location of ACC executable: {AccExecutablePath}", nameof(AccCompileTaskHandler), _task.InputFilePath, _task.OutputFilePath, path);

        // Verify the task has a name.
        if (_task.Name == null)
        {
            _taskContext.TaskOutput.Add(TaskOutputResult.CreateError("Task is missing a name."));
            return false;
        }

        // We get the arguments early in this task because we need the input for the error file handling.
        var (inputPath, outputPath) = GetArguments();

        var inputPathDirectory = Path.GetDirectoryName(inputPath);
        if (string.IsNullOrWhiteSpace(inputPathDirectory))
        {
            throw new ArgumentNullException(nameof(inputPath), "Expected a valid directory path from the input path.");
        }
        
        var errorFilePath = Path.Combine(inputPathDirectory, "acs.err");
        var errorFileRenamePathTemplate = CompositeFormat.Parse(Path.Combine(inputPathDirectory, "acs.err.backup{0}"));
        
        // Check for existing 'acs.err' file.
        // If found, move this file because otherwise the compiler gets rid of it.
        if (File.Exists(errorFilePath))
        {
            File.Move(errorFilePath, GetValidBackupPath(errorFileRenamePathTemplate));
        }

        using var stdOutStream = new MemoryStream();
        using var stdErrStream = new MemoryStream();

        bool succeeded;
        try
        {
            succeeded = await TaskHelper.RunProcessAsync(
                path,
                new[] { inputPath, outputPath }.Select(x => $"\"{x}\""),
                stdOutStream,
                stdErrStream,
                HandleStdout,
                HandleStdErr);
        }

        // Premature exception was thrown, not related to compilation.
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

        // Check for `acs.err`. Return its content as an error.
        var compileResult = await HandleAccErrAsync(errorFilePath);
        if (compileResult != null)
        {
            _taskContext.TaskOutput.Add(TaskOutputResult.CreateError(compileResult));
        }

        // The compiler failed.
        if (!succeeded)
        {
            _taskContext.TaskOutput.Add(TaskOutputResult.CreateError($"Compilation failed."));
            return false;
        }

        return true;
    }
    
    private string GetAccCompilerExecutableFilePath()
    {
        var path = _invokeContext.ContextBag.GetAccCompilerExecutableFilePath();
        
        // Handle relative path
        if (!Path.IsPathRooted(path))
        {
            if (string.IsNullOrWhiteSpace(_invokeContext.WorkingDirectory))
            {
                throw new ArgumentException($"Failed to update relative ACC compiler executable path ({path}). No working directory was specified. Either the working directory must be specified or the ACC compiler executable path must be absolute.");
            }
            
            path = Path.Combine(_invokeContext.WorkingDirectory, path);
        }
        
        return path;
    }

    private void HandleStdout(string line)
    {
        // We do not use the stdout because ACC logs a lot of unneeded messages.
    }

    private void HandleStdErr(string line)
    {
        // We do not use the stderr because ACC logs a lot of unneeded messages.
        // Intead we parse the 'acs.err' file that comes with errors.
    }

    private (string inputPath, string outputPath) GetArguments()
    {
        var inputPath = _task.InputFilePath;
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath, nameof(_task.InputFilePath));
        
        // Handle relative input path
        if (!Path.IsPathRooted(inputPath))
        {
            if (string.IsNullOrWhiteSpace(_invokeContext.WorkingDirectory))
            {
                throw new ArgumentException($"Failed to update relative input path for ACS file input ({inputPath}). No working directory was specified. Either the working directory must be specified or the path to the ACS file input must be absolute.");
            }
            
            inputPath = Path.Combine(_invokeContext.WorkingDirectory, inputPath);
        }
        
        var outputPath = _task.OutputFilePath;
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath, nameof(_task.OutputFilePath));
        
        // Handle relative output path
        if (!Path.IsPathRooted(outputPath))
        {
            if (string.IsNullOrWhiteSpace(_invokeContext.WorkingDirectory))
            {
                throw new ArgumentException($"Failed to update relative input path for ACS file output ({outputPath}). No working directory was specified. Either the working directory must be specified or the path to the ACS file output must be absolute.");
            }
            
            outputPath = Path.Combine(_invokeContext.WorkingDirectory, outputPath);
        }
        
        return (inputPath, outputPath);
    }

    private string GetValidBackupPath(CompositeFormat errorFileRenamePathTemplate)
    {
        var path = string.Format(CultureInfo.InvariantCulture, errorFileRenamePathTemplate, string.Empty);
        var nextFileIndex = 0;
        while (File.Exists(path))
        {
            nextFileIndex++;
            path = string.Format(CultureInfo.InvariantCulture, errorFileRenamePathTemplate, $" ({nextFileIndex})");
        }

        return path;
    }

    private async ValueTask<string?> HandleAccErrAsync(string errorFilePath)
    {
        if (!File.Exists(errorFilePath))
        {
            return null;
        }

        string content;
        using (var reader = new StreamReader(errorFilePath))
        {
            content = await reader.ReadToEndAsync();
        }

        // If the error file should not be kept then remove the 'acs.err' file.
        if (!_task.KeepAccErrFile)
        {
            _logger.LogDebug("Remove 'acs.err' file as configured.");
            File.Delete(errorFilePath);
        }

        return content;
    }
}