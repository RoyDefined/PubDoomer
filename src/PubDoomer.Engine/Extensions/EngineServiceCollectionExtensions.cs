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
            .AddTransient<ProjectTaskOrchestrator>();
    }

    private static ProjectTaskOrchestratorProviderDelegate GetTaskHandlerOrDefault(IServiceProvider provider)
    {
        return (Type taskHandlerType, IRunnableTask task, TaskInvokeContext context)
            => ActivatorUtilities.CreateInstance(provider, taskHandlerType, task, context) as ITaskHandler;
    }
}