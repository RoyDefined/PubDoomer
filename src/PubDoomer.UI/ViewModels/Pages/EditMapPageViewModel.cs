using System;
using Avalonia.Controls;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace PubDoomer.ViewModels.Pages;

public class EditMapPageViewModel : PageViewModel
{
    private readonly ILogger _logger;

    public EditMapPageViewModel()
    {
        if (!Design.IsDesignMode) throw new InvalidOperationException();

        _logger = NullLogger.Instance;
    }

    public EditMapPageViewModel(
        ILogger<EditMapPageViewModel> logger)
    {
        _logger = logger;
        _logger.LogDebug("Created.");
    }
}