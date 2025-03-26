using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PubDoomer.Engine.Orchestration;

namespace PubDoomer.Engine.Orchestrator;

public delegate ITaskHandler? ProjectTaskOrchestratorProviderDelegate(Type handlerType, EngineTaskBase task, PublishingContext context);