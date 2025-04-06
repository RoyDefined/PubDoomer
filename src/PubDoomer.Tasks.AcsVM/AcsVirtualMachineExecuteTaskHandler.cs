using Microsoft.Extensions.Logging;
using PubDoomer.Engine.TaskInvokation.Context;
using PubDoomer.Engine.TaskInvokation.Orchestration;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using PubDoomer.Engine.TaskInvokation.Utils;
using PubDoomer.Tasks.AcsVM.Extensions;

namespace PubDoomer.Tasks.AcsVM;

public sealed class AcsVirtualMachineExecuteTaskHandler : ITaskHandler
{
    private readonly ILogger _logger;
    private readonly IInvokableTask _taskContext;
    private readonly TaskInvokeContext _invokeContext;
    private readonly AcsVirtualMachineExecuteTask _task;

    public AcsVirtualMachineExecuteTaskHandler(
        ILogger<AcsVirtualMachineExecuteTaskHandler> logger, IInvokableTask taskContext, TaskInvokeContext invokeContext)
    {
        if (taskContext.Task is not AcsVirtualMachineExecuteTask task)
        {
            throw new ArgumentException($"The given task is not a {nameof(AcsVirtualMachineExecuteTask)}.");
        }

        _logger = logger;
        _taskContext = taskContext;
        _invokeContext = invokeContext;
        _task = task;
    }

    public async ValueTask<bool> HandleAsync()
    {
        var path = _invokeContext.ContextBag.GetAcsVmExecutableFilePath();
        _logger.LogDebug("Invoking {TaskName}. Input path: {InputFilePath}. Location of ACSVM executable: {AccExecutablePath}", nameof(AcsVirtualMachineExecuteTaskHandler), _task.InputFilePath, path);

        // Create streams for stdout and stderr.
        using var stdOutStream = new MemoryStream();
        using var stdErrStream = new MemoryStream();

        bool succeeded;
        try
        {

            succeeded = await TaskHelper.RunProcessAsync(
                path,
                BuildArguments(),
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

        // The compiler failed.
        if (!succeeded)
        {
            _taskContext.TaskOutput.Add(TaskOutputResult.CreateError("Code execution failed."));
            return false;
        }

        return true;
    }

    private void HandleStdout(string line)
    {
        // Anything passed to stdout is part of code output.
        _taskContext.TaskOutput.Add(TaskOutputResult.CreateMessage(line));
    }

    private void HandleStdErr(string line)
    {
        // Warn here as the compiler likely failed and this will be logged as an error.
        _taskContext.TaskOutput.Add(TaskOutputResult.CreateWarning(line));
    }

    private IEnumerable<string> BuildArguments()
    {
        yield return $"\"{_task.InputFilePath}\"";
    }
}