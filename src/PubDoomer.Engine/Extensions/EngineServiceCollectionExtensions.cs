using Microsoft.Extensions.DependencyInjection;
using PubDoomer.Engine.Orchestration;
using PubDoomer.Engine.Orchestrator;

namespace PubDoomer.Engine.Extensions;

public static class EngineServiceCollectionExtensions
{
    public static IServiceCollection AddEngine(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddTransient<ProjectTaskOrchestratorProviderDelegate>(x => (type, task, context) =>
                ActivatorUtilities.CreateInstance(x, type, task, context) as ITaskHandler)
            .AddTransient<ProjectTaskOrchestrator>();
    }
}