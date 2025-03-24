using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Project.Engine;

namespace PubDoomer.Engine;

/// <summary>
/// Represents the base of an engine's configuration to run with.
/// </summary>
public abstract partial class EngineRunConfiguration : ObservableObject
{
    // Set during creation, never `null`.
    public EngineContext Context { get; set; } = null!;
    
    public abstract IEnumerable<string> GetCommandLineArguments();
}