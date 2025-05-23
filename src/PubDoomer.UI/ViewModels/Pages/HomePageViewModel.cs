﻿using System;
using Avalonia.Controls;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace PubDoomer.ViewModels.Pages;

public class HomePageViewModel : PageViewModel
{
    private readonly ILogger _logger;

    public HomePageViewModel()
    {
        if (!Design.IsDesignMode) throw new InvalidOperationException();

        _logger = NullLogger.Instance;
    }

    public HomePageViewModel(
        ILogger<HomePageViewModel> logger)
    {
        _logger = logger;
        _logger.LogDebug("Created.");
    }
}