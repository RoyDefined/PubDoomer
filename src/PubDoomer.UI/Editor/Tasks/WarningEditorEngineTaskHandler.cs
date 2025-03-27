using System.Threading.Tasks;
using PubDoomer.Engine.TaskInvokation.Context;
using PubDoomer.Engine.TaskInvokation.Task;
using PubDoomer.UI.Editor.Tasks;

namespace PubDoomer.UI.Editor.Tasks;

public sealed class WarningEditorEngineTaskHandler(
    WarningEditorEngineTask _,
    TaskInvokeContext __) : ITaskHandler
{
    public ValueTask<TaskInvokationResult> HandleAsync()
    {
        return ValueTask.FromResult(
            TaskInvokationResult.FromSuccess(
                "The warning task has invoked succesfully.",
                ["Warning 1", "Warning 2"]));
    }
}