using System;
using System.Threading.Tasks;
using PubDoomer.Engine.TaskInvokation.Context;
using PubDoomer.Engine.TaskInvokation.Orchestration;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using PubDoomer.UI.Editor.Tasks;

namespace PubDoomer.UI.Editor.Tasks;

public sealed class WarningEditorTaskHandler(IInvokableTask taskContext, TaskInvokeContext _) : ITaskHandler
{
    public ValueTask<bool> HandleAsync()
    {
        taskContext.TaskOutput.Add(TaskOutputResult.CreateWarning("Warning 1"));
        taskContext.TaskOutput.Add(TaskOutputResult.CreateWarning("Warning 2"));
        taskContext.TaskOutput.Add(TaskOutputResult.CreateSuccess("The success task has invoked succesfully."));
        return ValueTask.FromResult(true);
    }
}