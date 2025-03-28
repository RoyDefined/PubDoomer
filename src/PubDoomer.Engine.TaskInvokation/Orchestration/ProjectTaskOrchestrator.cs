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

    private ITaskValidator GetTaskValidator(IRunnableTask task)
    {
        var handler = validatorProviderDelegate(task);
        if (handler == null)
            throw new ArgumentException($"Unknown validator type: {task.HandlerType.FullName}");

        return handler;
    }

    private async Task<TaskInvokationResult> InvokeTaskAsync(IRunnableTask task, TaskInvokeContext context)
    {
        var handler = handlerProviderDelegate(task, context);
        if (handler == null)
            throw new ArgumentException($"Unknown handler type: {task.HandlerType.FullName}");

        logger.LogDebug("Invoking task {TaskName}", task.HandlerType.FullName);
        return await handler.HandleAsync();
    }
}