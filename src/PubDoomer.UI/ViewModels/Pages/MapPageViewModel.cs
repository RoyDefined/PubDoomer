using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PubDoomer.Project;
using PubDoomer.Project.Maps;

namespace PubDoomer.ViewModels.Pages;

public partial class MapPageViewModel : PageViewModel
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

    [RelayCommand]
    public async Task EditMapAsync(MapContext map)
    {
        // TODO
    }
}