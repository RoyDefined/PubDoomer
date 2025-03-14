using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PubDoomer.Engine.Orchestration;

public delegate ITaskHandler? ProjectTaskOrchestratorProviderDelegate(Type handlerType, EngineTaskBase task, PublishingContext context);