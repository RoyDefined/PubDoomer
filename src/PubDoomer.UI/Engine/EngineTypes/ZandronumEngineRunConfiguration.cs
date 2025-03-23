using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Project.Engine;

namespace PubDoomer.Engine;

/// <summary>
/// Represents the base configuration for Zandronum engines to run with.
/// </summary>
public sealed partial class ZandronumEngineRunConfiguration : ZdoomDerivedEngineRunConfiguration
{
    /// <summary>
    /// The file path of configuration to apply to the running game.
    /// </summary>
    [ObservableProperty] private string? _configurationFilePath;
    
    /// <summary>
    /// The game mode to run the game in.
    /// </summary>
    [ObservableProperty] private string? _gameMode;


    public override IEnumerable<string> GetCommandLineArguments()
    {
        foreach(var arg in base.GetCommandLineArguments()) yield return arg;
        
        if (ConfigurationFilePath != null) yield return $"+exec \"{ConfigurationFilePath}\"";
        if (GameMode != null) yield return $"+{GameMode} true";
    }
}