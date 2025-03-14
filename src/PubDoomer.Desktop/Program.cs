using System;
using Avalonia;
using PubDoomer.Context;

namespace PubDoomer.Desktop;

public sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        var appBuilder = BuildAvaloniaApp();
        appBuilder.StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        var prodiver = PubDoomerContextBuilder.BuildDefaultProvider();
        return AppBuilder.Configure(() => new App(prodiver))
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    }
}