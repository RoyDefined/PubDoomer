using System.Collections.ObjectModel;
using Avalonia.Collections;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Project.Engine;
using PubDoomer.Project.IWad;
using PubDoomer.Tasks.AcsVM.Utils;
using PubDoomer.Tasks.Compile.Utils;
using PubDoomer.Utils;

namespace PubDoomer.Saving;

public partial class LocalSettings : ObservableObject
{
    // Configurable executions
    [ObservableProperty] private AvaloniaDictionary<string, string> _configurations = new();
    
    /// <summary>
    /// Configurable locations of game engines.
    /// </summary>
    [ObservableProperty] private ObservableCollection<EngineContext> _engines = [];

    /// <summary>
    /// Configurable locations of IWad files.
    /// </summary>
    [ObservableProperty] private ObservableCollection<IWadContext> _iWads = [];

    public LocalSettings()
    {
        if (!Design.IsDesignMode) return;
        
        // Add design-time data.
        Configurations = new()
        {
            [CompileTaskStatics.AccCompilerExecutableFilePathKey] = "Path/To/ACC.exe",
            [CompileTaskStatics.BccCompilerExecutableFilePathKey] = "Path/To/BCC.exe",
            [CompileTaskStatics.GdccAccCompilerExecutableFilePathKey] = "Path/To/GDCC-ACC.exe",
            [SavingStatics.SladeExecutableFilePathKey] = "Path/To/Slade.exe",
            [SavingStatics.UdbExecutableFilePathKey] = "Path/To/UtimateDoombuilder.exe",
            [AcsVmTaskStatics.AcsVmExecutableFilePathKey] = "Path/To/ACS-VM.exe",
        };

        Engines =
        [
            new EngineContext()
            {
                Name = "Zandronum (latest)",
                Path = "Path/To/Zandronum.exe",
                Type = EngineType.Zandronum,
            },
            new EngineContext()
            {
                Name = "GZDoom (latest)",
                Path = "Path/To/GZDoom.exe",
                Type = EngineType.GzDoom,
            },
            new EngineContext()
            {
                Name = "ZDoom (latest)",
                Path = "Path/To/ZDoom.exe",
                Type = EngineType.Zdoom,
            },
            new EngineContext()
            {
                Name = "Pending engine",
                Path = "Path/To/Something.exe",
                Type = EngineType.Unknown,
            }
        ];
            
        IWads =
        [
            new IWadContext()
            {
                Name = "Doom 2",
                Path = "Path/To/Doom2.wad",
            },
            new IWadContext()
            {
                Name = "Doom (shareware)",
                Path = "Path/To/Doom.wad",
            }
        ];
    }
}