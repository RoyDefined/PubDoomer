using Microsoft.Extensions.Logging;
using PubDoomer.Engine.TaskInvokation.Context;
using PubDoomer.Engine.TaskInvokation.Task;

namespace PubDoomer.Engine.TaskInvokation.Orchestration;

public sealed class ProjectTaskOrchestrator(
    ILogger<ProjectTaskOrchestrator> logger,
    ProjectTaskOrchestratorProviderDelegate projectTaskOrchestratorProviderDelegate)
{
    public async Task<TaskInvokationResult> InvokeTaskAsync(IRunnableTask task, TaskInvokeContext context)
    {
        var handler = projectTaskOrchestratorProviderDelegate(task.HandlerType, task, context);
        if (handler == null)
            throw new ArgumentException($"Unknown handler type: {task.HandlerType.FullName}");

        logger.LogDebug("Invoking task {TaskName}", task.HandlerType.FullName);
        return await handler.HandleAsync();
    }
}