using System.Collections.ObjectModel;

namespace PubDoomer.Engine.TaskHandling;

public sealed class TaskInvokationResult
{
    private TaskInvokationResult()
    {
    }

    private static readonly TaskInvokationResult Success = new TaskInvokationResult { ResultType = TaskResultType.Success };

    public required TaskResultType ResultType { get; init; }
    public string ResultMessage { get; init; } = "The task completed succesfully.";
    public ICollection<string>? Warnings { get; init; }
    public ICollection<string>? Errors { get; init; }
    public Exception? Exception { get; init; }

    public static TaskInvokationResult FromSuccess()
    {
        return Success;
    }

    public bool IsSuccess => ResultType == TaskResultType.Success;

    public static TaskInvokationResult FromSuccess(string message, IList<string>? warnings = null)
    {
        return new TaskInvokationResult
        {
            ResultType = TaskResultType.Success,
            ResultMessage = message,
            Warnings = warnings != null ? new Collection<string>(warnings) : null
        };
    }

    public static TaskInvokationResult FromError(string message, string? error = null, Exception? exception = null)
    {
        return new TaskInvokationResult
        {
            ResultType = TaskResultType.Error,
            ResultMessage = message,
            Errors = error != null ? new Collection<string>([error]) : null,
            Exception = exception
        };
    }

    public static TaskInvokationResult FromErrors(string message, IList<string>? warnings = null, IList<string>? errors = null)
    {
        return new TaskInvokationResult
        {
            ResultType = TaskResultType.Error,
            ResultMessage = message,
            Warnings = warnings != null ? new Collection<string>(warnings) : null,
            Errors = errors != null ? new Collection<string>(errors) : null,
        };
    }
}