using CommunityToolkit.Mvvm.ComponentModel;

namespace PubDoomer.Project.Maps;

/// <summary>
/// Represents the configured context of a map entry in a project.
/// </summary>
public partial class MapContext : ObservableObject
{
    /// <summary>
    /// The path to this map.
    /// </summary>
    [ObservableProperty] private string? _path;
    
    /// <summary>
    /// A name to give to this map to show in the UI.
    /// </summary>
    [ObservableProperty] private string? _name;
    
    /// <summary>
    /// The defined name of the map as specified on the map lump, which is used to navigate to the map (e.g. using <c>ChangeMap {name}</c>).
    /// </summary>
    [ObservableProperty] private string? _mapLumpName;
}