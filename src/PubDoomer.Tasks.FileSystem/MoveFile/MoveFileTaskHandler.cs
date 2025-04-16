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

namespace PubDoomer.Tasks.FileSystem.MoveFile;

public sealed class MoveFileTaskHandler : ITaskHandler
{
    private readonly ILogger _logger;
    private readonly IInvokableTask _taskContext;
    private readonly TaskInvokeContext _invokeContext;

    private readonly ObservableMoveFileTask _task;

    public MoveFileTaskHandler(
        ILogger<MoveFileTaskHandler> logger, IInvokableTask taskContext, TaskInvokeContext invokeContext)
    {
        if (taskContext.Task is not ObservableMoveFileTask task)
        {
            throw new ArgumentException($"The given task is not a {nameof(ObservableMoveFileTask)}.");
        }

        _logger = logger;
        _taskContext = taskContext;
        _invokeContext = invokeContext;

        _task = task;
    }

    public ValueTask<bool> HandleAsync()
    {
        // TODO
        return ValueTask.FromResult(true);
    }
}