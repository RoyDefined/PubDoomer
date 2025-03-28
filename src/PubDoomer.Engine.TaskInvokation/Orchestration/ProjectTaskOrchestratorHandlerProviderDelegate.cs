using PubDoomer.Engine.TaskInvokation.Context;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;

namespace PubDoomer.Engine.TaskInvokation.Orchestration;

public delegate ITaskHandler? ProjectTaskOrchestratorHandlerProviderDelegate(IRunnableTask task, TaskInvokeContext context);