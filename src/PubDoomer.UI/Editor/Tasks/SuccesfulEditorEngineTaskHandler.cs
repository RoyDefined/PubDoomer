using System.Threading.Tasks;
using PubDoomer.Engine.Orchestration;
using PubDoomer.Engine.TaskHandling;
using PubDoomer.UI.Editor.Tasks;

namespace PubDoomer.UI.Editor.Tasks;

public sealed class SuccesfulEditorEngineTaskHandler(
    SuccesfulEditorEngineTask taskInfo,
    PublishingContext context) : ITaskHandler
{
    public ValueTask<TaskInvokationResult> HandleAsync()
    {
        return ValueTask.FromResult(TaskInvokationResult.FromSuccess("The success task has invoked succesfully."));
    }
}