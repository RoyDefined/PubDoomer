using CommunityToolkit.Mvvm.ComponentModel;

namespace PubDoomer.Project.Engine;

/// <summary>
/// Represents an engine used for running defined maps.
/// </summary>
public partial class EngineContext : ObservableObject
{
    /// <summary>
    /// The path to this engines.
    /// </summary>
    [ObservableProperty] private string? _path;
    
    /// <summary>
    /// A name to give to this engine to show in the UI.
    /// </summary>
    [ObservableProperty] private string? _name;
    
    /// <summary>
    /// The type of engine defined.
    /// </summary>
    [ObservableProperty] private EngineType _type;
}