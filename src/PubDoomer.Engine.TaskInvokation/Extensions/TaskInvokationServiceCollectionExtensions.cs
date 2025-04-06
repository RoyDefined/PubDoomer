using Microsoft.Extensions.DependencyInjection;
using PubDoomer.Engine.TaskInvokation.Orchestration;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using System.Threading.Tasks;

namespace PubDoomer.Engine.TaskInvokation.Extensions;

public static class TaskInvokationServiceCollectionExtensions
{
    public static IServiceCollection AddTaskInvokation(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddTransient(GetTaskHandlerOrDefault)
            .AddTransient(GetTaskValidatorOrDefault)
            .AddTransient<ProjectTaskOrchestrator>();
    }

    private static ProjectTaskOrchestratorHandlerProviderDelegate GetTaskHandlerOrDefault(IServiceProvider provider)
    {
        return (task, context)
            => ActivatorUtilities.CreateInstance(provider, task.Task.HandlerType, task, context) as ITaskHandler;
    }

    private static ProjectTaskOrchestratorValidatorProviderDelegate GetTaskValidatorOrDefault(IServiceProvider provider)
    {
        return (task)
            => ActivatorUtilities.CreateInstance(provider, task.ValidatorType!, task) as ITaskValidator;
    }
}