using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PubDoomer.Engine.TaskHandlers;

namespace PubDoomer.Engine.Orchestration;

public sealed class ProjectTaskOrchestrator(
    ILogger<ProjectTaskOrchestrator> logger,
    ProjectTaskOrchestratorProviderDelegate projectTaskOrchestratorProviderDelegate)
{
    public async Task<TaskInvokationResult> InvokeTaskAsync(EngineTaskBase task, PublishingContext context)
    {
        var handler = projectTaskOrchestratorProviderDelegate(task.HandlerType, task, context);
        if (handler == null)
            throw new ArgumentException($"Unknown handler type: {task.HandlerType.FullName}");

        logger.LogDebug("Invoking task {TaskName}", task.HandlerType.FullName);
        return await handler.HandleAsync();
    }
}