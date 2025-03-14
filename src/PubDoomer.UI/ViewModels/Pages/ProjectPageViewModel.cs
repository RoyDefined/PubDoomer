using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PubDoomer.Project;
using PubDoomer.Services;

namespace PubDoomer.ViewModels.Pages;

public partial class ProjectPageViewModel : PageViewModel
{
    private static readonly Dictionary<string, string> _typeToMessage = new()
    {
        ["AccCompiler"] = "Select ACC compiler executable",
        ["BccCompiler"] = "Select BCC compiler executable",
        ["GdccCompiler"] = "Select GDCC compiler executable",
        ["Udb"] = "Select Ultimate Doombuilder executable",
        ["Slade"] = "Select Slade executable",
        ["AcsVm"] = "Select ACS VM executable"
    };

    private readonly ILogger _logger;
    private readonly WindowProvider? _windowProvider;

    public ProjectPageViewModel()
    {
        if (!Design.IsDesignMode) throw new InvalidOperationException();

        _logger = NullLogger.Instance;
        CurrentProjectProvider = new CurrentProjectProvider();
    }

    public ProjectPageViewModel(
        ILogger<ProjectPageViewModel> logger,
        CurrentProjectProvider currentProjectProvider,
        WindowProvider windowProvider)
    {
        _logger = logger;
        CurrentProjectProvider = currentProjectProvider;
        _windowProvider = windowProvider;

        _logger.LogDebug("Created.");
    }
    
    public CurrentProjectProvider CurrentProjectProvider { get; }

    [RelayCommand]
    private async Task PickFileAsync(string type)
    {
        if (AssertInDesignMode()) return;
        Debug.Assert(CurrentProjectProvider.ProjectContext != null);

        var window = _windowProvider.ProvideWindow();
        var filePicker = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = _typeToMessage[type],
            AllowMultiple = false
        });

        if (filePicker.Count == 0) return;

        // TODO: Simplify this into a dictionary.
        var filePath = filePicker.First().Path.LocalPath;
        switch (type)
        {
            case "AccCompiler":
                CurrentProjectProvider.ProjectContext.AccCompilerExecutableFilePath = filePath;
                break;
            case "BccCompiler":
                CurrentProjectProvider.ProjectContext.BccCompilerExecutableFilePath = filePath;
                break;
            case "GdccCompiler":
                CurrentProjectProvider.ProjectContext.GdccCompilerExecutableFilePath = filePath;
                break;
            case "Udb":
                CurrentProjectProvider.ProjectContext.UdbExecutableFilePath = filePath;
                break;
            case "Slade":
                CurrentProjectProvider.ProjectContext.SladeExecutableFilePath = filePath;
                break;
            case "AcsVm":
                CurrentProjectProvider.ProjectContext.AcsVmExecutableFilePath = filePath;
                break;

            default:
                throw new UnreachableException();
        }
    }

    [MemberNotNullWhen(false, nameof(_windowProvider))]
    private bool AssertInDesignMode()
    {
        return Design.IsDesignMode;
    }
}