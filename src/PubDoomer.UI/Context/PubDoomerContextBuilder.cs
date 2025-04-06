using System;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PubDoomer.Encryption;
using PubDoomer.Engine.Extensions;
using PubDoomer.Engine.TaskInvokation.Context;
using PubDoomer.Factory;
using PubDoomer.Logging;
using PubDoomer.Project;
using PubDoomer.Saving;
using PubDoomer.Services;
using PubDoomer.Settings.Local;
using PubDoomer.Settings.Project;
using PubDoomer.Settings.Recent;
using PubDoomer.ViewModels;
using Serilog;

namespace PubDoomer.Context;

public static class PubDoomerContextBuilder
{
    public static IServiceProvider BuildDefaultProvider()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);

        var configuration = new ConfigurationManager();
        ConfigureConfigurationBase(configuration);

        var environment = ConfigureEnvironment(configuration);
        ConfigureConfiguration(configuration, environment);

        services.AddSingleton(environment);
        services.AddSingleton(configuration);
        return services.BuildServiceProvider();
    }

    private static void ConfigureServices(ServiceCollection services)
    {
        // Logging.
        _ = services.AddLogging();
        _ = services.AddSerilog((serviceProvider, loggerConfiguration) =>
        {
            var logEmitter = serviceProvider.GetRequiredService<LogEmitter>();
            var sink = new LogEmitterSink(logEmitter);

            loggerConfiguration
                .MinimumLevel.Debug()
                .WriteTo.Sink(sink);
        });
        _ = services.AddSingleton<LogEmitter>();

        // Notifications.
        // This service is a singleton so each instance shares the same notification layer.
        _ = services.AddSingleton<WindowNotificationManager>(x =>
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                return new WindowNotificationManager(desktop.MainWindow)
                {
                    Position = NotificationPosition.BottomRight
                };

            throw new ArgumentException("Could not determine lifetime for window notification manager.");
        });

        // Saving
        _ = services.AddTransient<ProjectSavingService>()
            .AddTransient<RecentProjectsService>()
            .AddTransient<LocalSettingsService>()
            .AddTransient<EncryptionService>()
            .AddSingleton<RecentProjectCollection>()
            .AddSingleton<LocalSettings>()
            .AddSingleton<SessionSettings>();

        // General services
        _ = services.AddSingleton<CurrentProjectProvider>()
            .AddTransient<DialogueProvider>()
            .AddTransient<WindowProvider>();

        // The main view model of the application is a singleton as its reference is used in other view models.
        _ = services

            // The Window model bases off the view model so both share the same instance.
            // Depending on the lifetime one or the other is used.
            .AddSingleton<MainWindowModel>()
            .AddSingleton(x => (MainViewModel)x.GetRequiredService<MainWindowModel>())
            .AddPageViewModels()
            .AddEngine();
    }

    private static void ConfigureConfigurationBase(ConfigurationManager configuration)
    {
        configuration.AddEnvironmentVariables("PD_");
    }

    private static PubDoomerEnvironment ConfigureEnvironment(ConfigurationManager configuration)
    {
        var environment = new PubDoomerEnvironment(
            configuration[PubDoomerEnvironment.EnvironmentKey] ?? PubDoomerEnvironment.DefaultEnvironment,
            Assembly.GetEntryAssembly()!.GetName().Name!);

        return environment;
    }

    private static void ConfigureConfiguration(ConfigurationManager configuration, PubDoomerEnvironment environment)
    {
        // The environment json must be optional in design mode due to missing environment variables causing issues otherwise.
        var environmentJsonOptional = Design.IsDesignMode;
        configuration.AddJsonFile("appsettings.json", false, true)
            .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", environmentJsonOptional, true);
    }
}