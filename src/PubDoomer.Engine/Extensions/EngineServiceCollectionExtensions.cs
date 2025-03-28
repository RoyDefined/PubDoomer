using Microsoft.Extensions.DependencyInjection;
using PubDoomer.Engine.TaskInvokation.Context;
using PubDoomer.Engine.TaskInvokation.Orchestration;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using System.Threading.Tasks;

namespace PubDoomer.Engine.Extensions;

public static class EngineServiceCollectionExtensions
{
    public static IServiceCollection AddEngine(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddTransient(GetTaskHandlerOrDefault)
            .AddTransient(GetTaskValidatorOrDefault)
            .AddTransient<ProjectTaskOrchestrator>();
    }

    private static ProjectTaskOrchestratorHandlerProviderDelegate GetTaskHandlerOrDefault(IServiceProvider provider)
    {
        return (IRunnableTask task, TaskInvokeContext context)
            => ActivatorUtilities.CreateInstance(provider, task.HandlerType, task, context) as ITaskHandler;
    }

    private static ProjectTaskOrchestratorValidatorProviderDelegate GetTaskValidatorOrDefault(IServiceProvider provider)
    {
        return (IRunnableTask task)
            => ActivatorUtilities.CreateInstance(provider, task.ValidatorType!, task) as ITaskValidator;
    }
}