using Microsoft.Extensions.Logging;
using PubDoomer.Engine.TaskInvokation.Context;
using PubDoomer.Engine.TaskInvokation.Orchestration;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using PubDoomer.Engine.TaskInvokation.Utils;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading.Channels;

namespace PubDoomer.Tasks.CopyProject;

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
        // Does the project already point to the target path?
        // If so, end early.

        // Copy over the project to the new location.
        
        // Update the current location of the project.

        return true;
    }
}