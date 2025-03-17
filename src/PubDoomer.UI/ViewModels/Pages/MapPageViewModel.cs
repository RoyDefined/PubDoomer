using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PubDoomer.Project;
using PubDoomer.Project.Maps;
using PubDoomer.Saving;
using PubDoomer.Services;
using PubDoomer.Utils.MergedSettings;
using PubDoomer.ViewModels.Dialogues;

namespace PubDoomer.ViewModels.Pages;

public partial class MapPageViewModel : PageViewModel
{
    private readonly ILogger _logger;
    private readonly LocalSettings _settings;
    private readonly DialogueProvider? _dialogueProvider;

    public MapPageViewModel()
    {
        if (!Design.IsDesignMode) throw new InvalidOperationException();

        _logger = NullLogger.Instance;
        CurrentProjectProvider = new CurrentProjectProvider();
        _settings = new LocalSettings();
    }

    public MapPageViewModel(
        ILogger<MapPageViewModel> logger,
        CurrentProjectProvider currentProjectProvider,
        LocalSettings localSettings,
        DialogueProvider dialogueProvider)
    {
        _logger = logger;
        CurrentProjectProvider = currentProjectProvider;
        _settings = localSettings;
        _dialogueProvider = dialogueProvider;

        _logger.LogDebug("Created.");
    }
    
    public CurrentProjectProvider CurrentProjectProvider { get; }

    [RelayCommand]
    private async Task EditMapAsync(MapContext map)
    {
        if (AssertInDesignMode()) return;
        
        // Get the current settings context so we can determine the location of Ultimate Doombuilder.
        var settings = SettingsMerger.Merge(CurrentProjectProvider.ProjectContext, _settings);
        
        _logger.LogDebug("Opening map '{MapName}' using Ultimate Doombuilder configured at path '{UdbPath}'.", map.Name, settings.UdbExecutableFilePath ?? "N/A");
        
        // Path to UDB must exist.
        if (settings.UdbExecutableFilePath == null)
        {
            await _dialogueProvider.AlertAsync(AlertType.Warning, "Missing configuration",
                "The path to Ultimate Doombuilder is not configured. You must either configure the path in the project settings, or your local settings.");
            return;
        }
        
        // TODO: Launch UDB with the map.
    }

    [MemberNotNullWhen(false, nameof(_dialogueProvider))]
    private bool AssertInDesignMode()
    {
        return Design.IsDesignMode;
    }
}