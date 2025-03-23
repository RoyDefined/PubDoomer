using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Project.Engine;

namespace PubDoomer.Engine;

// TODO: Extend configuration with more options. https://zdoom.org/wiki/Command_line_parameters

/// <summary>
/// Represents the base configuration of an engine derived from the ZDoom engine to run with.
/// </summary>
public abstract partial class ZdoomDerivedEngineRunConfiguration : EngineRunConfiguration
{
    /* Debug */
    
    /// <summary>
    /// If <c>true</c>, quits the game just before video initialization. To be used to check for errors in scripts without actually running the game.
    /// </summary>
    [ObservableProperty] private bool _noRun;
    
    /// <summary>
    /// If <c>true</c>, sends all output to a system console in Windows.
    /// </summary>
    [ObservableProperty] private bool _stdOut;
    
    /// <summary>
    /// The file path for a log file to be written.
    /// </summary>
    [ObservableProperty] private string? _logFile;
    
    /* Loading */
    
    /// <summary>
    /// The skill to use when playing.
    /// </summary>
    [ObservableProperty] private int? _skill;
    
    /*  Multiplayer */
    
    /// <summary>
    /// If <c>true</c>, the game will function as a host. This allows for playing a local server.
    /// </summary>
    [ObservableProperty] private bool _multiplayer;
}