namespace PubDoomer.Engine.Compile.Gdcc;

public sealed class EngineGdccAccCompileTask : EngineCompileTaskBase
{
    private static readonly Type HandlerTypeCached = typeof(EngineGdccAccCompileTaskHandler);

    public required bool DontWarnForwardReferences { get; init; }
    
    public override Type HandlerType => HandlerTypeCached;
    protected override string[] ExpectedFileExtensions { get; } = [".acs", ".txt"];
}