using System;
using System.Threading.Tasks;
using PubDoomer.Engine.Orchestration;
using PubDoomer.Engine.TaskHandling;
using PubDoomer.UI.Editor.Tasks;

namespace PubDoomer.UI.Editor.Tasks;

public sealed class ErrorEditorEngineTaskHandler(
    ErrorEditorEngineTask taskInfo,
    TaskInvokeContext context) : ITaskHandler
{
    public ValueTask<TaskInvokationResult> HandleAsync()
    {
        return ValueTask.FromResult(
            TaskInvokationResult.FromError("The error task had an error.", "Testing 1, 2, 3.", new Exception("Hello, World!")));
    }
}