using Microsoft.Extensions.DependencyInjection;
using PubDoomer.Engine.TaskInvokation.Context;
using PubDoomer.Engine.TaskInvokation.Extensions;
using PubDoomer.Engine.TaskInvokation.Orchestration;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using System.Threading.Tasks;

namespace PubDoomer.Engine.Extensions;

public static class EngineServiceCollectionExtensions
{
    public static IServiceCollection AddEngine(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddTaskInvokation();
    }
}