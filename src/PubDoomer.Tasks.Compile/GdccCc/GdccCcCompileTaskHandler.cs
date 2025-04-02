using Microsoft.Extensions.Logging;
using PubDoomer.Engine.TaskInvokation.Context;
using PubDoomer.Engine.TaskInvokation.Process;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using PubDoomer.Tasks.Compile.Extensions;

namespace PubDoomer.Tasks.Compile.GdccAcc;

public sealed class GdccCcCompileTaskHandler(
    ILogger<GdccCcCompileTaskHandler> logger,
    ObservableGdccCcCompileTask taskInfo,
    TaskInvokeContext context) : ProcessInvokeHandlerBase(logger, taskInfo), ITaskHandler
{
    private const string CompileResultWarningPrefix = "WARNING: ";
    private const string CompileResultErrorPrefix = "ERROR: ";

    protected override string StdOutFileName => "stdout_gdcc-cc.txt";
    protected override string StdErrFileName => "stderr_gdcc-cc.txt";

    public async ValueTask<TaskInvokationResult> HandleAsync()
    {
        var gdccCcPath = context.ContextBag.GetGdccCcCompilerExecutableFilePath();
        logger.LogDebug("Invoking {TaskName}. Input path: {InputFilePath}. Output path: {OutputFilePath}. Location of GDCC-CC executable: {GdccCcExecutablePath}", nameof(GdccCcCompileTaskHandler), taskInfo.InputFilePath, taskInfo.OutputFilePath, gdccCcPath);

        // TODO
        throw new NotImplementedException();
    }
}