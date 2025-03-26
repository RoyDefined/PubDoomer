using System.Threading.Tasks;
using PubDoomer.Engine.Orchestration;
using PubDoomer.Engine.TaskHandling;
using PubDoomer.UI.Editor.Tasks;

namespace PubDoomer.UI.Editor.Tasks;

public sealed class WarningEditorEngineTaskHandler(
    WarningEditorEngineTask taskInfo,
    PublishingContext context) : ITaskHandler
{
    public ValueTask<TaskInvokationResult> HandleAsync()
    {
        return ValueTask.FromResult(
            TaskInvokationResult.FromSuccess(
                "The warning task has invoked succesfully.",
                ["Warning 1", "Warning 2"]));
    }
}