using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PubDoomer.Project;
using PubDoomer.Project.Archive;
using PubDoomer.Project.Engine;
using PubDoomer.Project.IWad;
using PubDoomer.Services;
using PubDoomer.ViewModels.Dialogues;

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
        ["AcsVm"] = "Select ACS VM executable",
        ["Zandronum"] = "Select Zandronum executable"
    };

    private readonly ILogger _logger;
    private readonly WindowProvider? _windowProvider;
    private readonly WindowNotificationManager? _notificationManager;
    private readonly DialogueProvider? _dialogueProvider;

    public ProjectPageViewModel()
    {
        if (!Design.IsDesignMode) throw new InvalidOperationException();

        _logger = NullLogger.Instance;
        CurrentProjectProvider = new CurrentProjectProvider();
    }

    public ProjectPageViewModel(
        ILogger<ProjectPageViewModel> logger,
        CurrentProjectProvider currentProjectProvider,
        WindowProvider windowProvider,
        WindowNotificationManager notificationManager,
        DialogueProvider dialogueProvider)
    {
        _logger = logger;
        CurrentProjectProvider = currentProjectProvider;
        _windowProvider = windowProvider;
        _notificationManager = notificationManager;
        _dialogueProvider = dialogueProvider;

        _logger.LogDebug("Created.");
    }
    
    public CurrentProjectProvider CurrentProjectProvider { get; }
    
    [RelayCommand]
    private void AddEngine()
    {
        Debug.Assert(CurrentProjectProvider.ProjectContext != null);
        CurrentProjectProvider.ProjectContext.Engines.Add(new EngineContext());
    }

    [RelayCommand]
    private async Task DeleteEngineAsync(EngineContext context)
    {
        Debug.Assert(CurrentProjectProvider.ProjectContext != null);
    }

    [RelayCommand]
    private void AddArchive()
    {
        Debug.Assert(CurrentProjectProvider.ProjectContext != null);
        CurrentProjectProvider.ProjectContext.Archives.Add(new ArchiveContext());
    }

    [RelayCommand]
    private async Task DeleteArchiveAsync(ArchiveContext context)
    {
        Debug.Assert(CurrentProjectProvider.ProjectContext != null);

        // In design mode we delete without a prompt.
        if (AssertInDesignMode())
        {
            CurrentProjectProvider.ProjectContext.Archives.Remove(context);
            return;
        }
        
        var result = await _dialogueProvider.PromptAsync(
            AlertType.Warning,
            "Delete profile",
            "Are you sure you want to delete this archive?",
            "The archive will be deleted and you will have to readd it.",
            new InformationalWindowButton(AlertType.None, "Cancel"),
            new InformationalWindowButton(AlertType.Error, "Delete"));

        if (!result) return;

        CurrentProjectProvider.ProjectContext.Archives.Remove(context);
        _notificationManager?.Show(new Notification("Archive deleted", "The archive has been deleted.",
            NotificationType.Success));
    }
    
    [RelayCommand]
    private void AddIWad()
    {
        Debug.Assert(CurrentProjectProvider.ProjectContext != null);
        CurrentProjectProvider.ProjectContext.IWads.Add(new IWadContext());
    }

    [RelayCommand]
    private async Task DeleteIWadAsync(IWadContext context)
    {
        Debug.Assert(CurrentProjectProvider.ProjectContext != null);

        // In design mode we delete without a prompt.
        if (AssertInDesignMode())
        {
            CurrentProjectProvider.ProjectContext.IWads.Remove(context);
            return;
        }
        
        var result = await _dialogueProvider.PromptAsync(
            AlertType.Warning,
            "Delete IWad",
            "Are you sure you want to delete this IWad?",
            "The IWad will be deleted and you will have to readd it.",
            new InformationalWindowButton(AlertType.None, "Cancel"),
            new InformationalWindowButton(AlertType.Error, "Delete"));

        if (!result) return;

        CurrentProjectProvider.ProjectContext.IWads.Remove(context);
        _notificationManager?.Show(new Notification("IWad deleted", "The IWad has been deleted.",
            NotificationType.Success));
    }

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
            case "Zandronum":
                CurrentProjectProvider.ProjectContext.ZandronumExecutableFilePath = filePath;
                break;

            default:
                throw new UnreachableException();
        }
    }

    [MemberNotNullWhen(false, nameof(_windowProvider), nameof(_dialogueProvider), nameof(_notificationManager))]
    private bool AssertInDesignMode()
    {
        return Design.IsDesignMode;
    }
}