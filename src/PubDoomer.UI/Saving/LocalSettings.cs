﻿using System.Collections.ObjectModel;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Project.Engine;
using PubDoomer.Project.IWad;

namespace PubDoomer.Saving;

public partial class LocalSettings : ObservableObject
{
    // Configurable executions
    [ObservableProperty] private string? _accCompilerExecutableFilePath;
    [ObservableProperty] private string? _bccCompilerExecutableFilePath;
    [ObservableProperty] private string? _gdccCompilerExecutableFilePath;
    [ObservableProperty] private string? _sladeExecutableFilePath;
    [ObservableProperty] private string? _udbExecutableFilePath;
    [ObservableProperty] private string? _acsVmExecutableFilePath;
    
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
        AccCompilerExecutableFilePath = "Path/To/ACC.exe";
        BccCompilerExecutableFilePath = "Path/To/BCC.exe";
        GdccCompilerExecutableFilePath = "Path/To/GDCC-ACC.exe";
        SladeExecutableFilePath = "Path/To/Slade.exe";
        UdbExecutableFilePath = "Path/To/UtimateDoombuilder.exe";
        AcsVmExecutableFilePath = "Path/To/ACS-VM.exe";

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