namespace PubDoomer.Engine.Orchestration;

public struct ValidateResult
{

    private ValidateResult(
        ValidateResultType type,
        string message,
        Exception? exception)
    {
        Type = type;
        Message = message;
        Exception = exception;
    }
    
    public ValidateResultType Type { get; }
    public string Message { get; }
    public Exception? Exception { get; }

    public static ValidateResult FromWarning(string message)
    {
        return new ValidateResult(ValidateResultType.Warning, message, null);
    }

    public static ValidateResult FromError(string message)
    {
        return new ValidateResult(ValidateResultType.Error, message, null);
    }

    public static ValidateResult FromError(string message, Exception exception)
    {
        return new ValidateResult(ValidateResultType.Error, message, null);
    }
}