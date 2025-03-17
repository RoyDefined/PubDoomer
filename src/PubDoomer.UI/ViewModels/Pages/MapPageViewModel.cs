using System;
using Avalonia.Controls;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PubDoomer.Project;

namespace PubDoomer.ViewModels.Pages;

public class MapPageViewModel : PageViewModel
{
    private readonly ILogger _logger;

    public MapPageViewModel()
    {
        if (!Design.IsDesignMode) throw new InvalidOperationException();

        _logger = NullLogger.Instance;
        CurrentProjectProvider = new CurrentProjectProvider();
    }
    
    public CurrentProjectProvider CurrentProjectProvider { get; }

    public MapPageViewModel(
        ILogger<MapPageViewModel> logger,
        CurrentProjectProvider currentProjectProvider)
    {
        _logger = logger;
        CurrentProjectProvider = currentProjectProvider;
        
        _logger.LogDebug("Created.");
    }
}