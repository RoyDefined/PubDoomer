using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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

// TODO: IWad selection should also give the option for project IWads
public partial class MapPageViewModel : PageViewModel
{
    private readonly ILogger _logger;
    private readonly DialogueProvider? _dialogueProvider;
    private readonly WindowProvider? _windowProvider;
    private readonly WindowNotificationManager? _notificationManager;
    private readonly MergedSettings? _mergedSettings;

    [ObservableProperty] private IWadContext? _selectedIWad;

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
        
        _notificationManager?.Show(new Notification("Profile created", "The profile has been created succesfully.",
            NotificationType.Success));
    }

    [RelayCommand]
    private async Task EditMapAsync(MapContext map)
    {
        if (AssertInDesignMode()) return;
        Debug.Assert(CurrentProjectProvider.ProjectContext != null);
        
        _logger.LogDebug("Opening map '{MapName}' using Ultimate Doombuilder configured at path '{UdbPath}'.", map.Name, _mergedSettings.UdbExecutableFilePath ?? "N/A");
        
        // Path to UDB must exist.
        if (_mergedSettings.UdbExecutableFilePath == null)
        {
            await _dialogueProvider.AlertAsync(AlertType.Warning, "Missing configuration",
                "The path to Ultimate Doombuilder is not configured. You must either configure the path in the project settings, or your local settings.");
            return;
        }
        
        // Verify an IWad is selected.
        // If not, we open the dialogue to configure it and end this method.
        if (SelectedIWad == null)
        {
            await _dialogueProvider.AlertAsync(AlertType.Warning, "Select an IWad.");
            await ConfigureEditMapAsync(map);
            return;
        }
        
        await OpenMapAsync(_mergedSettings.UdbExecutableFilePath, map, SelectedIWad, CurrentProjectProvider.ProjectContext.Archives);
    }

    private async Task OpenMapAsync(string udbExecutableFilePath, MapContext map, IWadContext selectedIWad,
        ObservableCollection<ArchiveContext> archives)
    {
        Debug.Assert(_dialogueProvider != null);
        
        // Launch UDB with the map.
        // Any exceptions are displayed in a window.
        try
        {
            MapEditUtil.StartUltimateDoomBuilder(udbExecutableFilePath, map, selectedIWad, archives);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open Ultimate Doombuilder");
            await _dialogueProvider.AlertAsync(AlertType.Warning, "Failed to open Ultimate Doombuilder",
                $"An error occurred while opening Ultimate Doombuilder. Please check your configuration. Error: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ConfigureEditMapAsync(MapContext map)
    {
        if (AssertInDesignMode()) return;
        Debug.Assert(CurrentProjectProvider.ProjectContext != null);
        
        _logger.LogDebug("Configuring to map '{MapName}' using Ultimate Doombuilder configured at path '{UdbPath}'.", map.Name, _mergedSettings.UdbExecutableFilePath ?? "N/A");
        
        // Path to UDB must exist.
        if (_mergedSettings.UdbExecutableFilePath == null)
        {
            await _dialogueProvider.AlertAsync(AlertType.Warning, "Missing configuration",
                "The path to Ultimate Doombuilder is not configured. You must either configure the path in the project settings, or your local settings.");
            return;
        }
        
        // TODO: Provide IWads
        var vm = new ConfigureEditMapViewModel(_mergedSettings.UdbExecutableFilePath, []);
        var result = await _dialogueProvider.GetCreateOrEditDialogueWindowAsync(vm);
        if (!result || vm.SelectedIWad == null) return;
        
        SelectedIWad = vm.SelectedIWad;
        await OpenMapAsync(_mergedSettings.UdbExecutableFilePath, map, SelectedIWad, CurrentProjectProvider.ProjectContext.Archives);
    }

    [MemberNotNullWhen(false, nameof(_dialogueProvider), nameof(_windowProvider), nameof(_notificationManager), nameof(_mergedSettings))]
    private bool AssertInDesignMode()
    {
        return Design.IsDesignMode;
    }
}