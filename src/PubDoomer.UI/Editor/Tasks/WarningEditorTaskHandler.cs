using System.Threading.Tasks;
using PubDoomer.Engine.TaskInvokation.Context;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using PubDoomer.UI.Editor.Tasks;

namespace PubDoomer.UI.Editor.Tasks;

public sealed class WarningEditorTaskHandler(
    ObservableWarningEditorTask _,
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