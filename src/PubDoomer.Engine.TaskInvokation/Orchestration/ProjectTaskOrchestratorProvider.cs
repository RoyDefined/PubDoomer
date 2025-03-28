using PubDoomer.Engine.TaskInvokation.Context;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;

namespace PubDoomer.Engine.TaskInvokation.Orchestration;

public delegate ITaskHandler? ProjectTaskOrchestratorProviderDelegate(Type handlerType, IRunnableTask task, TaskInvokeContext context);