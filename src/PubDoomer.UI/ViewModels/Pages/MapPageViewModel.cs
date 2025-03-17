using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PubDoomer.Project;
using PubDoomer.Project.Maps;
using PubDoomer.Saving;
using PubDoomer.Services;
using PubDoomer.Utils.MergedSettings;
using PubDoomer.ViewModels.Dialogues;
using PubDoomer.Views.Dialogues;

namespace PubDoomer.ViewModels.Pages;

public partial class MapPageViewModel : PageViewModel
{
    private readonly ILogger _logger;
    private readonly LocalSettings _settings;
    private readonly DialogueProvider? _dialogueProvider;
    private readonly WindowProvider? _windowProvider;
    private readonly WindowNotificationManager? _notificationManager;

    public MapPageViewModel()
    {
        if (!Design.IsDesignMode) throw new InvalidOperationException();

        _logger = NullLogger.Instance;
        CurrentProjectProvider = new CurrentProjectProvider();
        SessionSettings = new SessionSettings();
        _settings = new LocalSettings();
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
        _settings = localSettings;
        _dialogueProvider = dialogueProvider;
        _windowProvider = windowProvider;
        _notificationManager = notificationManager;

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

    [MemberNotNullWhen(false, nameof(_dialogueProvider), nameof(_windowProvider), nameof(_notificationManager))]
    private bool AssertInDesignMode()
    {
        return Design.IsDesignMode;
    }
}