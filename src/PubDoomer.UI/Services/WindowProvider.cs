using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace PubDoomer.Services;

public sealed class WindowProvider
{
    internal Window ProvideWindow()
    {
        if (!TryProvideWindow(out var window))
            throw new ArgumentException("Failed to get the window in the current context.");

        return window;
    }

    internal bool TryProvideWindow([NotNullWhen(true)] out Window? window)
    {
        window = Application.Current?.ApplicationLifetime switch
        {
            IClassicDesktopStyleApplicationLifetime desktop => desktop.MainWindow,
            _ => null
        };

        return window != null;
    }

    internal TopLevel ProvideTopLevel()
    {
        if (!TryProvideTopLevel(out var topLevel))
            throw new ArgumentException("Failed to get the top level in the current context.");

        return topLevel;
    }

    internal bool TryProvideTopLevel([NotNullWhen(true)] out TopLevel? topLevel)
    {
        if (!TryProvideWindow(out var window))
        {
            topLevel = null;
            return false;
        }

        topLevel = TopLevel.GetTopLevel(window);
        return topLevel != null;
    }
}