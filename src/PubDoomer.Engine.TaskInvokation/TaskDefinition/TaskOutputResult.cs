using System.Collections.ObjectModel;

namespace PubDoomer.Engine.TaskInvokation.TaskDefinition;

public sealed class TaskOutputResult
{
    private TaskOutputResult(TaskOutputType type, string message, Exception? exception)
    {
        Type = type;
        Message = message;
        Exception = exception;
    }

    public TaskOutputType Type { get; }
    public string Message { get; }
    public Exception? Exception { get; }

    public static TaskOutputResult CreateMessage(string message) => new(TaskOutputType.Message, message, null);
    public static TaskOutputResult CreateSuccess(string message) => new(TaskOutputType.Success, message, null);
    public static TaskOutputResult CreateWarning(string message) => new(TaskOutputType.Warning, message, null);
    public static TaskOutputResult CreateError(string message) => new(TaskOutputType.Error, message, null);

    public static TaskOutputResult CreateWarning(string message, Exception exception) => new(TaskOutputType.Warning, message, exception);
    public static TaskOutputResult CreateError(string message, Exception exception) => new(TaskOutputType.Error, message, exception);
}