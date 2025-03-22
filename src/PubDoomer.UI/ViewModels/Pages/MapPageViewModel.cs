using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PubDoomer.Project;
using PubDoomer.Project.Archive;
using PubDoomer.Project.IWad;
using PubDoomer.Project.Maps;
using PubDoomer.Saving;
using PubDoomer.Services;
using PubDoomer.Utils;
using PubDoomer.Utils.MergedSettings;
using PubDoomer.ViewModels.Dialogues;
using PubDoomer.Views.Dialogues;

namespace PubDoomer.ViewModels.Pages;

public partial class MapPageViewModel : PageViewModel
{
    private readonly ILogger _logger;
    private readonly DialogueProvider? _dialogueProvider;
    private readonly WindowProvider? _windowProvider;
    private readonly WindowNotificationManager? _notificationManager;
    private readonly MergedSettings? _mergedSettings;

    [ObservableProperty] private IWadContext? _selectedIWad;
    [ObservableProperty] private string? _selectedConfiguration;

    private string[]? _selectableConfigurations;

    public MapPageViewModel()
    {
        if (!Design.IsDesignMode) throw new InvalidOperationException();

        _logger = NullLogger.Instance;
        CurrentProjectProvider = new CurrentProjectProvider();
        SessionSettings = new SessionSettings();
    }

    public MapPageViewModel(
        ILogger<MapPageViewModel> logger,
        CurrentProjectProvider currentProjectProvider,
        SessionSettings sessionSettings,
        LocalSettings localSettings,
        DialogueProvider dialogueProvider,
        WindowProvider windowProvider,
        WindowNotificationManager notificationManager)
    {
        _logger = logger;
        CurrentProjectProvider = currentProjectProvider;
        SessionSettings = sessionSettings;
        _dialogueProvider = dialogueProvider;
        _windowProvider = windowProvider;
        _notificationManager = notificationManager;
        
        // Get the current settings context so we can determine the location of Ultimate Doombuilder and the IWads.
        _mergedSettings = SettingsMerger.Merge(CurrentProjectProvider.ProjectContext, localSettings);

        _logger.LogDebug("Created.");
    }
    
    public CurrentProjectProvider CurrentProjectProvider { get; }
    public SessionSettings SessionSettings { get; }

    [RelayCommand]
    private async Task AddMapsAsync()
    {
        if (AssertInDesignMode()) return;

        var vm = new AddMapsWindowViewModel(_windowProvider);
        var result = await _dialogueProvider.GetCreateOrEditDialogueWindowAsync(vm);
        if (!result) return;

        Debug.Assert(CurrentProjectProvider.ProjectContext != null);

        foreach (var map in vm.MapsToAdd)
        {
            CurrentProjectProvider.ProjectContext.Maps.Add(map);
        }
        
        _notificationManager?.Show(new Notification("Profile created", "The profile has been created successfully.",
            NotificationType.Success));
    }

    [RelayCommand]
    private async Task EditMapAsync(MapContext map)
    {
        if (AssertInDesignMode()) return;
        Debug.Assert(CurrentProjectProvider.ProjectContext != null);
        
        _logger.LogDebug("Opening map '{MapName}' using Ultimate DoomBuilder configured at path '{UdbPath}'.", map.Name, _mergedSettings.UdbExecutableFilePath ?? "N/A");
        
        // Path to UDB must exist.
        if (_mergedSettings.UdbExecutableFilePath == null)
        {
            await _dialogueProvider.AlertAsync(AlertType.Warning, "Missing configuration",
                "The path to Ultimate DoomBuilder is not configured. You must either configure the path in the project settings, or your local settings.");
            return;
        }
        
        // Verify an IWad and configuration is selected.
        // If not, we open the dialogue to configure it and end this method.
        if (SelectedIWad == null || SelectedConfiguration == null)
        {
            await _dialogueProvider.AlertAsync(AlertType.Warning, "Select an IWad and configuration to edit the map with.");
            await ConfigureEditMapAsync(map);
            return;
        }
        
        await OpenMapAsync(_mergedSettings.UdbExecutableFilePath, map, SelectedIWad, SelectedConfiguration, CurrentProjectProvider.ProjectContext.Archives);
    }

    private async Task OpenMapAsync(
        string udbExecutableFilePath,
        MapContext map,
        IWadContext selectedIWad,
        string selectedConfiguration,
        ObservableCollection<ArchiveContext> archives)
    {
        Debug.Assert(_dialogueProvider != null);
        
        // Launch UDB with the map.
        // Any exceptions are displayed in a window.
        try
        {
            MapEditUtil.StartUltimateDoomBuilder(udbExecutableFilePath, map, selectedIWad, selectedConfiguration, archives);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open Ultimate DoomBuilder");
            await _dialogueProvider.AlertAsync(AlertType.Warning, "Failed to open Ultimate DoomBuilder",
                $"An error occurred while opening Ultimate DoomBuilder. Please check your configuration. Error: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ConfigureEditMapAsync(MapContext map)
    {
        if (AssertInDesignMode()) return;
        Debug.Assert(CurrentProjectProvider.ProjectContext != null);
        
        _logger.LogDebug("Configuring to map '{MapName}' using Ultimate DoomBuilder configured at path '{UdbPath}'.", map.Name, _mergedSettings.UdbExecutableFilePath ?? "N/A");
        
        // Path to UDB must exist.
        if (_mergedSettings.UdbExecutableFilePath == null)
        {
            await _dialogueProvider.AlertAsync(AlertType.Warning, "Missing configuration",
                "The path to Ultimate DoomBuilder is not configured. You must either configure the path in the project settings, or your local settings.");
            return;
        }
        
        // Check for selectable configurations.
        // If not set, initialize them.
        try
        {
            _selectableConfigurations ??= MapEditUtil.GetConfigurations(_mergedSettings.UdbExecutableFilePath).ToArray();
        }
        catch (Exception)
        {
            await _dialogueProvider.AlertAsync(AlertType.Warning, "Failed to retrieve configurations",
                "Failed to retrieve eligible configurations for Ultimate DoomBuilder. Make sure the configured path is valid and contains a '/Configurations' folder with configurations.");
            return;
        }
        
        var vm = new ConfigureEditMapViewModel(_mergedSettings.UdbExecutableFilePath, _mergedSettings.IWads, _selectableConfigurations, SelectedIWad, SelectedConfiguration);
        var result = await _dialogueProvider.GetCreateOrEditDialogueWindowAsync(vm);
        if (!result || vm.SelectedIWad == null || vm.SelectedConfiguration == null) return;
        
        SelectedIWad = vm.SelectedIWad;
        SelectedConfiguration = vm.SelectedConfiguration;
        await OpenMapAsync(_mergedSettings.UdbExecutableFilePath, map, SelectedIWad, SelectedConfiguration, CurrentProjectProvider.ProjectContext.Archives);
    }

    [MemberNotNullWhen(false, nameof(_dialogueProvider), nameof(_windowProvider), nameof(_notificationManager), nameof(_mergedSettings))]
    private bool AssertInDesignMode()
    {
        return Design.IsDesignMode;
    }
}