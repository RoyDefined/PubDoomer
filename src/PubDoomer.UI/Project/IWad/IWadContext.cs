using CommunityToolkit.Mvvm.ComponentModel;

namespace PubDoomer.Project.IWad;

/// <summary>
/// Represents the context of an IWad that is used as a base for editing and testing.
/// </summary>
public partial class IWadContext : ObservableObject
{
    /// <summary>
    /// The path to this IWad.
    /// </summary>
    [ObservableProperty] private string? _path;
    
    /// <summary>
    /// A name to give to this IWad to show in the UI.
    /// </summary>
    [ObservableProperty] private string? _name;
}