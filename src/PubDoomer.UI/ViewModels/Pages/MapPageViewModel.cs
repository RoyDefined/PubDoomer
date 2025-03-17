using System;
using Avalonia.Controls;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace PubDoomer.ViewModels.Pages;

public class MapPageViewModel : PageViewModel
{
    private readonly ILogger _logger;

    public MapPageViewModel()
    {
        if (!Design.IsDesignMode) throw new InvalidOperationException();

        _logger = NullLogger.Instance;
    }

    public MapPageViewModel(
        ILogger<MapPageViewModel> logger)
    {
        _logger = logger;
        _logger.LogDebug("Created.");
    }
}