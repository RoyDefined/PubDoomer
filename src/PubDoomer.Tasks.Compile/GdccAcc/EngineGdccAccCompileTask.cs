namespace PubDoomer.Tasks.Compile.GdccAcc;

public sealed class EngineGdccAccCompileTask : EngineCompileTaskBase
{
    private static readonly Type HandlerTypeCached = typeof(EngineGdccAccCompileTaskHandler);

    public required bool DontWarnForwardReferences { get; init; }
    
    public override Type HandlerType => HandlerTypeCached;
    
    public override CompilerType Type => CompilerType.GdccAcc;
    protected override string[] ExpectedFileExtensions { get; } = [".acs", ".txt"];
}