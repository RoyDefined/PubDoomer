using System;
using System.Threading.Tasks;
using PubDoomer.Engine.TaskInvokation.Context;
using PubDoomer.Engine.TaskInvokation.Orchestration;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using PubDoomer.UI.Editor.Tasks;

namespace PubDoomer.UI.Editor.Tasks;

public sealed class SuccesfulEditorTaskHandler(IInvokableTask taskContext, TaskInvokeContext _) : ITaskHandler
{
    public ValueTask<bool> HandleAsync()
    {
        taskContext.TaskOutput.Add(TaskOutputResult.CreateSuccess("The success task has invoked succesfully."));
        return ValueTask.FromResult(true);
    }
}