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
using PubDoomer.Tasks.AcsVM.Utils;
using PubDoomer.Tasks.Compile.Utils;
using PubDoomer.Utils;
using PubDoomer.ViewModels.Dialogues;

namespace PubDoomer.ViewModels.Pages;

public partial class ProjectPageViewModel : PageViewModel
{
    private static readonly Dictionary<string, string> _typeToMessage = new()
    {
        [CompileTaskStatics.AccCompilerExecutableFilePathKey] = "Select ACC compiler executable",
        [CompileTaskStatics.BccCompilerExecutableFilePathKey] = "Select BCC compiler executable",
        [CompileTaskStatics.GdccAccCompilerExecutableFilePathKey] = "Select GDCC-ACC compiler executable",
        [CompileTaskStatics.GdccCcCompilerExecutableFilePathKey] = "Select GDCC-CC compiler executable",
        [CompileTaskStatics.GdccMakeLibCompilerExecutableFilePathKey] = "Select GDCC-MakeLib compiler executable",
        [CompileTaskStatics.GdccLdCompilerExecutableFilePathKey] = "Select GDCC-LD compiler executable",
        [SavingStatics.UdbExecutableFilePathKey] = "Select Ultimate Doombuilder executable",
        [SavingStatics.SladeExecutableFilePathKey] = "Select Slade executable",
        [AcsVmTaskStatics.AcsVmExecutableFilePathKey] = "Select ACS VM executable",
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

        // In design mode we delete without a prompt.
        if (AssertInDesignMode())
        {
            CurrentProjectProvider.ProjectContext.Engines.Remove(context);
            return;
        }
        
        var result = await _dialogueProvider.PromptAsync(
            AlertType.Warning,
            "Delete engine",
            "Are you sure you want to delete this engine?",
            "The engine will be deleted and you will have to readd it.",
            new InformationalWindowButton(AlertType.None, "Cancel"),
            new InformationalWindowButton(AlertType.Error, "Delete"));

        if (!result) return;

        CurrentProjectProvider.ProjectContext.Engines.Remove(context);
        _notificationManager?.Show(new Notification("Engine deleted", "The engine has been deleted.",
            NotificationType.Success));
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
    private async Task PickFileAsync(string configurationKey)
    {
        if (AssertInDesignMode()) return;
        Debug.Assert(CurrentProjectProvider.ProjectContext != null);

        var window = _windowProvider.ProvideWindow();
        var filePicker = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = _typeToMessage[configurationKey],
            AllowMultiple = false
        });

        if (filePicker.Count == 0) return;

        var filePath = filePicker.First().Path.AbsolutePath;
        CurrentProjectProvider.ProjectContext.Configurations[configurationKey] = filePath;
    }

    [MemberNotNullWhen(false, nameof(_windowProvider), nameof(_dialogueProvider), nameof(_notificationManager))]
    private bool AssertInDesignMode()
    {
        return Design.IsDesignMode;
    }
}