using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PubDoomer.Context;
using PubDoomer.ViewModels;
using PubDoomer.Views;

namespace PubDoomer;

public class App(
    IServiceProvider provider) : Application
{
    private readonly ILogger _logger = provider.GetRequiredService<ILogger<App>>();

    public override void Initialize()
    {
        _logger.LogDebug("Initializing.");
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        _logger.LogDebug("Framework initialized.");

        // Log additional information.
        var environment = provider.GetRequiredService<PubDoomerEnvironment>();
        _logger.LogDebug("Application name: {ApplicationName}.", environment.ApplicationName);
        _logger.LogDebug("Environment: {Environment}.", environment.EnvironmentName);

        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:

                // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
                // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
                DisableAvaloniaDataAnnotationValidation();

                // Explicitly create the main window, then the data context.
                // This gives dependencies a main window (and top level) to work with.
                desktop.MainWindow = new MainWindow();
                desktop.MainWindow.DataContext = provider.GetRequiredService<MainWindowModel>();

                break;

            // Used by the designer.
            case ISingleViewApplicationLifetime singleViewPlatform:

                singleViewPlatform.MainView = new MainView
                {
                    DataContext = provider.GetRequiredService<MainViewModel>()
                };
                break;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove) BindingPlugins.DataValidators.Remove(plugin);
    }
}