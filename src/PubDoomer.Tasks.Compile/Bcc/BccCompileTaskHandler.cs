using Microsoft.Extensions.Logging;
using PubDoomer.Engine.TaskInvokation.Context;
using PubDoomer.Engine.TaskInvokation.Orchestration;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using PubDoomer.Tasks.Compile.Extensions;
using PubDoomer.Tasks.Compile.Utils;
using System.Diagnostics;
using System.Text;
using SystemProcess = System.Diagnostics.Process;

namespace PubDoomer.Tasks.Compile.Bcc;

public sealed class BccCompileTaskHandler : ITaskHandler
{
    private readonly ILogger _logger;
    private readonly IInvokableTask _taskContext;
    private readonly TaskInvokeContext _invokeContext;
    private readonly ObservableBccCompileTask _task;

    public BccCompileTaskHandler(
        ILogger<BccCompileTaskHandler> logger, IInvokableTask taskContext, TaskInvokeContext invokeContext)
    {
        if (taskContext.Task is not ObservableBccCompileTask task)
        {
            throw new ArgumentException($"The given task is not a {nameof(ObservableBccCompileTask)}.");
        }

        _logger = logger;
        _taskContext = taskContext;
        _invokeContext = invokeContext;
        _task = task;
    }

    public async ValueTask<bool> HandleAsync()
    {
        var path = _invokeContext.ContextBag.GetBccCompilerExecutableFilePath();
        _logger.LogDebug("Invoking {TaskName}. Input path: {InputFilePath}. Output path: {OutputFilePath}. Location of BCC executable: {BccExecutablePath}", nameof(BccCompileTaskHandler), _task.InputFilePath, _task.OutputFilePath, path);

        // Verify the task has a name.
        if (_task.Name == null)
        {
            _taskContext.TaskOutput.Add(TaskOutputResult.CreateError("Task is missing a name."));
            return false;
        }

        using var stdOutStream = new MemoryStream();
        using var stdErrStream = new MemoryStream();

        bool succeeded;
        try
        {
            succeeded = await StartProcessAsync(path, BuildArguments(), stdOutStream, stdErrStream);
        }

        // Premature exception was thrown, not related to compilation.
        // TODO: Continue if possible and check if we also have compiler errors, should the process have executed succesfully?
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

        // The compiler returned an error.
        if (!succeeded)
        {
            // Check stderr for content, which means compilation failed.
            var compileResult = await HandleCompileErrorAsync(stdErrStream);
            if (compileResult == null)
            {
                _taskContext.TaskOutput.Add(TaskOutputResult.CreateError("Compilation failed for unknown reason."));
                return false;
            }

            _taskContext.TaskOutput.Add(TaskOutputResult.CreateError($"Compilation failed. {compileResult}"));
            return false;
        }

        return true;
    }

    private IEnumerable<string> BuildArguments()
    {
        yield return $"\"{_task.InputFilePath}\"";
        yield return $"\"{_task.OutputFilePath}\"";
    }

    private async Task<bool> StartProcessAsync(string path, IEnumerable<string> arguments, MemoryStream stdOutStream, MemoryStream stdErrStream)
    {
        _logger.LogDebug("calling process.");

        var processStartInfo = new ProcessStartInfo
        {
            FileName = path,
            Arguments = string.Join(" ", arguments),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new SystemProcess { StartInfo = processStartInfo };
        process.Start();

        // Additional task to output stdout and stderr.
        var outputTask = process.StandardOutput.BaseStream.CopyToAsync(stdOutStream);
        var errorTask = process.StandardError.BaseStream.CopyToAsync(stdErrStream);
        await Task.WhenAll(outputTask, errorTask, process.WaitForExitAsync());

        _logger.LogDebug("Process exit code: {ExitCode}", process.ExitCode);
        return process.ExitCode == 0;
    }

    private async ValueTask<string?> HandleCompileErrorAsync(Stream stream)
    {
        _ = stream.Seek(0, SeekOrigin.Begin);

        string content;
        using (var reader = new StreamReader(stream))
        {
            content = await reader.ReadToEndAsync();
        }

        // If the content is empty, there was no error.
        if (string.IsNullOrEmpty(content))
        {
            return null;
        }
        
        return content;
    }
}