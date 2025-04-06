using System;
using System.Threading.Tasks;
using PubDoomer.Engine.TaskInvokation.Context;
using PubDoomer.Engine.TaskInvokation.Orchestration;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using PubDoomer.UI.Editor.Tasks;

namespace PubDoomer.UI.Editor.Tasks;

public sealed class ErrorEditorTaskHandler(IInvokableTask taskContext, TaskInvokeContext _) : ITaskHandler
{
    public ValueTask<bool> HandleAsync()
    {
        taskContext.TaskOutput.Add(TaskOutputResult.CreateError("Testing 1, 2, 3.", new Exception("Hello, World!")));
        return ValueTask.FromResult(false);
    }
}