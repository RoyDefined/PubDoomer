namespace PubDoomer.Engine.Orchestration;

public abstract class EngineTaskBase
{
    public abstract Type HandlerType { get; }
    public required string Name { get; init; }
    
    public abstract IEnumerable<ValidateResult> Validate();
}