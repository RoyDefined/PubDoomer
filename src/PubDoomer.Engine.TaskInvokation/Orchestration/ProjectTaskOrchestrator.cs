using Microsoft.Extensions.Logging;
using PubDoomer.Engine.TaskInvokation.Context;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace PubDoomer.Engine.TaskInvokation.Orchestration;

public sealed class ProjectTaskOrchestrator(
    ILogger<ProjectTaskOrchestrator> logger,
    ProjectTaskOrchestratorProviderDelegate projectTaskOrchestratorProviderDelegate)
{
    public async System.Threading.Tasks.Task InvokeProfileAsync(IInvokableProfile profile, TaskInvokeContext context)
    {
        // TODO: Make use of the status.
        var stopwatch = Stopwatch.GetTimestamp();
        profile.Status = ProfileRunContextStatus.Running;

        foreach (var runTask in profile.Tasks)
        {
            runTask.Status = ProfileRunTaskStatus.Running;

            var result = await InvokeTaskAsync(runTask.Task, context);
            logger.LogDebug("Task result: {ResultType}, {Result}", result.ResultType, result.ResultMessage);

            runTask.Status = result.ResultType == TaskResultType.Success
                ? ProfileRunTaskStatus.Success
                : ProfileRunTaskStatus.Error;

            runTask.ResultMessage = result.ResultMessage;
            runTask.ResultWarnings = result.Warnings != null ? new ObservableCollection<string>(result.Warnings) : null;
            runTask.ResultErrors = result.Errors != null ? new ObservableCollection<string>(result.Errors) : null;
            runTask.Exception = result.Exception;

            // Check error behaviour.
            // If there was an error and the behaviour is to quit, then end the task invocation early.
            if (runTask.Status == ProfileRunTaskStatus.Error)
            {
                if (runTask.Behaviour == ProfileTaskErrorBehaviour.StopOnError)
                {
                    logger.LogWarning(runTask.Exception, "Task failure.");
                    profile.Status = ProfileRunContextStatus.Error;
                    break;
                }
                else
                {
                    logger.LogWarning(runTask.Exception, "Task failed but is configured to not stop on errors. Execution will continue.");
                }
            }
        }

        profile.ElapsedTimeMs = (int)Stopwatch.GetElapsedTime(stopwatch).TotalMilliseconds;
        if (profile.Status != ProfileRunContextStatus.Error)
        {
            profile.Status = ProfileRunContextStatus.Success;
        }
    }

    private async Task<TaskInvokationResult> InvokeTaskAsync(IRunnableTask task, TaskInvokeContext context)
    {
        var handler = projectTaskOrchestratorProviderDelegate(task.HandlerType, task, context);
        if (handler == null)
            throw new ArgumentException($"Unknown handler type: {task.HandlerType.FullName}");

        logger.LogDebug("Invoking task {TaskName}", task.HandlerType.FullName);
        return await handler.HandleAsync();
    }
}