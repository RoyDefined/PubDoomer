namespace PubDoomer.Tasks.Compile.Bcc;

public sealed class EngineBccCompileTask : EngineCompileTaskBase
{
    private static readonly Type HandlerTypeCached = typeof(EngineBccCompileTaskHandler);

    public override Type HandlerType => HandlerTypeCached;
    
    public override CompilerType Type => CompilerType.Bcc;
    protected override string[] ExpectedFileExtensions { get; } = [".acs", ".bcs", ".txt"];
}