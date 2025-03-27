using PubDoomer.Engine.TaskInvokation.Context;
using PubDoomer.Engine.TaskInvokation.Task;

namespace PubDoomer.Engine.TaskInvokation.Orchestration;

public delegate ITaskHandler? ProjectTaskOrchestratorProviderDelegate(Type handlerType, EngineTaskBase task, TaskInvokeContext context);