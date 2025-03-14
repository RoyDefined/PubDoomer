namespace PubDoomer.Engine.Compile.Bcc;

public sealed class EngineBccCompileTask : EngineCompileTaskBase
{
    private static readonly Type HandlerTypeCached = typeof(EngineBccCompileTaskHandler);

    public override Type HandlerType => HandlerTypeCached;
    protected override string[] ExpectedFileExtensions { get; } = [".acs", ".bcs", ".txt"];
}