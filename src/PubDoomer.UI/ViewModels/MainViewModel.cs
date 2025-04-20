using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PubDoomer.Context;
using PubDoomer.Factory;
using PubDoomer.Logging;
using PubDoomer.Project;
using PubDoomer.Project.Profile;
using PubDoomer.Project.Tasks;
using PubDoomer.Saving;
using PubDoomer.ViewModels.Pages;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace PubDoomer.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly PageViewModelFactory _pageViewModelFactory;
    private protected readonly ILogger Logger;
    private protected readonly WindowNotificationManager? WindowNotificationManager;
    private protected readonly PubDoomerEnvironment? Environment;

    [ObservableProperty] private SessionSettings _sessionSettings;

    // The current page to display in the main content.
    [ObservableProperty] private ViewModelBase? _currentPage;

    // The currently selected project.
    [ObservableProperty] private CurrentProjectProvider _currentProjectProvider;

    // List of logs for the logging container.
    [ObservableProperty] private ObservableCollection<string> _logs = [];

    // The normalized name of the current page.
    // Used to avoid setting the same page.
    private string? _normalizedPageName;

    // If true, expand the sidebar.
    [ObservableProperty] private bool _sideBarOpened;

    public MainViewModel()
    {
        if (!Design.IsDesignMode) throw new InvalidOperationException();

        Logger = NullLogger.Instance;
        CurrentProjectProvider = new CurrentProjectProvider();
        SessionSettings = new SessionSettings();

        // For now we ignore any calls to the factory, but in the future we could implement explicit creation of view models.
        // It would work a lot better in the designer if the pages loaded properly, but instead of DI we just make an empty view model.
        _pageViewModelFactory = new PageViewModelFactory(_ => null);

        ShowLogContainer = true;

        // Default to the home page.
        _currentPage = new HomePageViewModel();

        // Design time log messages.
        _logs.Add("Foo");
        _logs.Add("Bar");
        _logs.Add("Baz");
    }

    public MainViewModel(
        ILogger<MainViewModel> logger,
        LogEmitter logEmitter,
        PubDoomerEnvironment environment,
        SessionSettings sessionSettings,
        CurrentProjectProvider currentProjectProvider,
        PageViewModelFactory pageViewModelFactory,
        WindowNotificationManager windowNotificationManager)
    {
        Logger = logger;
        _pageViewModelFactory = pageViewModelFactory;
        SessionSettings = sessionSettings;
        CurrentProjectProvider = currentProjectProvider;
        WindowNotificationManager = windowNotificationManager;
        Environment = environment;
        
        ShowLogContainer = Environment.IsDevelopment;
        
        // Global INPC because the project is the only thing that can be changed.
        CurrentProjectProvider.PropertyChanged += (_, _) =>
        {
            WindowNotificationManager?.Show(
                new Notification(null, $"Changed the current project to {CurrentProjectProvider.ProjectContext!.Name}."));
        };

        // Default to the home page.
        _ = OpenPageAsync("Home");

        while (logEmitter.LogMessages.TryDequeue(out var logMessage)) _logs.Add(logMessage);
        logEmitter.LogEmitted += (_, e) => Logs.Add(e.LogMessage);

        Logger.LogDebug("Created.");
    }

    // If true, show the log container.
    // As the container is a development thing we only enable it in development or in the designer.
    public bool ShowLogContainer { get; }

    [RelayCommand]
    private void ToggleSideBar()
    {
        SideBarOpened = !SideBarOpened;
    }

    [RelayCommand]
    private async Task OpenPageAsync(string pageName)
    {
        var normalizedPageName = pageName.ToLowerInvariant();
        if (_normalizedPageName == normalizedPageName) return;

        _normalizedPageName = normalizedPageName;

        var page = _pageViewModelFactory.CreatePageViewModel(pageName);
        await DisposeCurrentPageAsync();

        // Ignore no page in the designer.
        // This always happens until we explicitly implement the factory.
        if (page == null)
        {
            if (Design.IsDesignMode) return;

            throw new ArgumentException(
                $"Failed to retrieve the page view model. No view model found under keyed name {pageName}.",
                nameof(pageName));
        }

        CurrentPage = page;
    }

    private async ValueTask DisposeCurrentPageAsync()
    {
        if (CurrentPage == null) return;
        
        switch (CurrentPage)
        {
            // ReSharper disable SuspiciousTypeConversion.Global
            case IAsyncDisposable asyncDisposable:
                await asyncDisposable.DisposeAsync();
                break;
            case IDisposable disposable:
                disposable.Dispose();
                break;
            // ReSharper restore SuspiciousTypeConversion.Global
        }

        CurrentPage = null;
    }
}