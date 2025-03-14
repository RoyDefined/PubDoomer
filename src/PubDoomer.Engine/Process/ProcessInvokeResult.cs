namespace PubDoomer.Engine.Process;

public readonly struct ProcessInvokeResult
{
    public int? ExitCode { get; init; }
    public Exception? Exception { get; init; }

    public bool HasCompilerError => ExitCode != 0;

    public static ProcessInvokeResult Create(int exitCode)
    {
        return new ProcessInvokeResult()
        {
            ExitCode = exitCode,
        };
    }

    public static ProcessInvokeResult Create(Exception exception)
    {
        return new ProcessInvokeResult()
        {
            Exception = exception,
        };
    }
}