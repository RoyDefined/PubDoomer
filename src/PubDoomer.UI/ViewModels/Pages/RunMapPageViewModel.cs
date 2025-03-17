using System;
using Avalonia.Controls;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace PubDoomer.ViewModels.Pages;

public class RunMapPageViewModel : PageViewModel
{
    private readonly ILogger _logger;

    public RunMapPageViewModel()
    {
        if (!Design.IsDesignMode) throw new InvalidOperationException();

        _logger = NullLogger.Instance;
    }

    public RunMapPageViewModel(
        ILogger<RunMapPageViewModel> logger)
    {
        _logger = logger;
        _logger.LogDebug("Created.");
    }
}