using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PubDoomer.Saving;
using PubDoomer.Services;

namespace PubDoomer.ViewModels.Pages;

// TODO: Save dark mode setting
// TODO: Use startup behaviour, also save startup behaviour.
public partial class SettingsPageViewModel : PageViewModel
{
    private static readonly Dictionary<string, string> _typeToMessage = new()
    {
        ["AccCompiler"] = "Select ACC compiler executable",
        ["BccCompiler"] = "Select BCC compiler executable",
        ["GdccCompiler"] = "Select GDCC compiler executable",
        ["Udb"] = "Select Ultimate Doombuilder executable",
        ["Slade"] = "Select Slade executable",
        ["AcsVm"] = "Select ACS VM executable",
        ["Zandronum"] = "Select Zandronum executable"
    };

    private readonly ILogger _logger;
    private readonly SavingService? _savingService;
    private readonly WindowProvider? _windowProvider;
    private readonly WindowNotificationManager? _notificationManager;

    public SettingsPageViewModel()
    {
        if (!Design.IsDesignMode) throw new InvalidOperationException();

        _logger = NullLogger.Instance;
        Settings = new LocalSettings();
    }

    public SettingsPageViewModel(
        ILogger<SettingsPageViewModel> logger,
        WindowProvider windowProvider,
        WindowNotificationManager notificationManager,
        SavingService savingService,
        LocalSettings settings)
    {
        _logger = logger;
        _windowProvider = windowProvider;
        _notificationManager = notificationManager;
        _savingService = savingService;

        Settings = settings;

        _logger.LogDebug("Created.");
    }

    public LocalSettings Settings { get; }

    public bool DarkModeEnabled => Application.Current!.ActualThemeVariant == ThemeVariant.Dark;

    [RelayCommand]
    private void ToggleTheme()
    {
        var app = Application.Current;
        if (app == null)
        {
            Debug.Fail("App should not be null.");
            return;
        }

        var targetTheme = DarkModeEnabled ? ThemeVariant.Light : ThemeVariant.Dark;
        _logger.LogDebug("Setting new theme to {Theme}.", targetTheme);
        app.RequestedThemeVariant = targetTheme;
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(DarkModeEnabled)));
    }

    [RelayCommand]
    private async Task PickFileAsync(string type)
    {
        if (AssertInDesignMode()) return;

        var window = _windowProvider.ProvideWindow();
        var filePicker = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = _typeToMessage[type],
            AllowMultiple = false
        });

        if (filePicker.Count == 0) return;

        var filePath = filePicker.First().Path.LocalPath;
        switch (type)
        {
            case "AccCompiler":
                Settings.AccCompilerExecutableFilePath = filePath;
                break;
            case "BccCompiler":
                Settings.BccCompilerExecutableFilePath = filePath;
                break;
            case "GdccCompiler":
                Settings.GdccCompilerExecutableFilePath = filePath;
                break;
            case "Udb":
                Settings.UdbExecutableFilePath = filePath;
                break;
            case "Slade":
                Settings.SladeExecutableFilePath = filePath;
                break;
            case "AcsVm":
                Settings.AcsVmExecutableFilePath = filePath;
                break;
            case "Zandronum":
                Settings.ZandronumExecutableFilePath = filePath;
                break;

            default:
                throw new UnreachableException();
        }
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        if (AssertInDesignMode()) return;

        await _savingService.SaveLocalSettingsAsync();
        _notificationManager.Show(new Notification("Settings saved", "The settings have been updated succesfully.",
            NotificationType.Success));
    }

    [MemberNotNullWhen(false, nameof(_windowProvider), nameof(_savingService), nameof(_notificationManager))]
    private bool AssertInDesignMode()
    {
        return Design.IsDesignMode;
    }
}