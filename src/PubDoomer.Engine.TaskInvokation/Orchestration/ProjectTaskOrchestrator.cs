using Microsoft.Extensions.Logging;
using PubDoomer.Engine.TaskInvokation.Context;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using PubDoomer.Engine.TaskInvokation.Validation;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace PubDoomer.Engine.TaskInvokation.Orchestration;

public sealed class ProjectTaskOrchestrator(
    ILogger<ProjectTaskOrchestrator> logger,
    ProjectTaskOrchestratorHandlerProviderDelegate handlerProviderDelegate,
    ProjectTaskOrchestratorValidatorProviderDelegate validatorProviderDelegate)
{
    /// <summary>
    /// Fully validates the current profile to ensure it can run properly.
    /// <br/> Validation is before the tasks are invoked. This means it does not guarantee the tasks run succesfully at runtime.
    /// </summary>
    public TaskValidationCollection[] ValidateProfile(IInvokableProfile profile)
    {
        var validationResultsNullable = profile.Tasks
            .Where(x => x.Task.ValidatorType != null)
            .Select(x =>
            {
                var validatableTask = GetTaskValidator(x.Task);

                var results = validatableTask.Validate();
                var collection = new TaskValidationCollection(x, results.ToArray());

                // Return `null` if no validation results were found.
                // This is filtered below.
                if (collection.Results.Count == 0)
                {
                    return null;
                }

                return collection;
            });

        return validationResultsNullable
            .Where(x => x != null)
            .Cast<TaskValidationCollection>()
            .ToArray();
    }

    public async Task InvokeProfileAsync(IInvokableProfile profile, TaskInvokeContext context)
    {
        // TODO: Make use of the status.
        var stopwatch = Stopwatch.GetTimestamp();
        profile.Status = ProfileRunContextStatus.Running;

        foreach (var runTask in profile.Tasks)
        {
            runTask.Status = ProfileRunTaskStatus.Running;

            var success = await InvokeTaskAsync(runTask, context);
            runTask.Status = success ? ProfileRunTaskStatus.Success : ProfileRunTaskStatus.Error;

            logger.LogDebug("Task success: {Success}", success);

            // Check error behaviour.
            // If there was an error and the behaviour is to quit, then end the task invocation early.
            if (runTask.Status == ProfileRunTaskStatus.Error)
            {
                if (runTask.Behaviour == ProfileTaskErrorBehaviour.StopOnError)
                {
                    profile.Status = ProfileRunContextStatus.Error;
                    logger.LogWarning("Task failure. Profile is configured to stop.");
                    break;
                }
                else
                {
                    logger.LogWarning("Task failed but is configured to not stop on errors. Execution will continue.");
                    continue;
                }
            }

            var warningCount = runTask.TaskOutput.Count(x => x.Type == TaskOutputType.Warning);
            var errorCount = runTask.TaskOutput.Count(x => x.Type == TaskOutputType.Error);
            var message = (warningCount, errorCount) switch
            {
                (0, 0) => "Task succeeded",
                (_, 0) => $"Task succeeded with {warningCount} warning(s).",
                (0, _) => $"Task succeeded with {errorCount} error(s).",
                (_, _) => $"Task succeeded with {warningCount} warning(s) and {errorCount} error(s).",
            };

            runTask.TaskOutput.Add(TaskOutputResult.CreateSuccess(message));
        }

        profile.ElapsedTimeMs = (int)Stopwatch.GetElapsedTime(stopwatch).TotalMilliseconds;
        if (profile.Status != ProfileRunContextStatus.Error)
        {
            profile.Status = ProfileRunContextStatus.Success;
        }
    }

    private ITaskValidator GetTaskValidator(IRunnableTask task)
    {
        var handler = validatorProviderDelegate(task);
        if (handler == null)
            throw new ArgumentException($"Unknown validator type: {task.HandlerType.FullName}");

        return handler;
    }

    private async Task<bool> InvokeTaskAsync(IInvokableTask taskContext, TaskInvokeContext context)
    {
        var handler = handlerProviderDelegate(taskContext, context);
        if (handler == null)
            throw new ArgumentException($"Unknown handler type: {taskContext.Task.HandlerType.FullName}");

        logger.LogDebug("Invoking task {TaskName}", taskContext.Task.HandlerType.FullName);
        return await handler.HandleAsync();
    }
}