using CommunityToolkit.Mvvm.ComponentModel;

namespace PubDoomer.Project.Archive;

public partial class ArchiveContext : ObservableObject
{
    /// <summary>
    /// The path to this archive.
    /// </summary>
    [ObservableProperty] private string? _path;
    
    /// <summary>
    /// A name to give to this archive to show in the UI.
    /// </summary>
    [ObservableProperty] private string? _name;
    
    /// <summary>
    /// If <see langword="true"/>, exclude this archive from testing parameters in Ultimate DoomBuilder or when testing the project.
    /// </summary>
    [ObservableProperty] private bool _excludeFromTesting;
}