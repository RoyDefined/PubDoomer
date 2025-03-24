using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Engine;
using PubDoomer.Project;
using PubDoomer.Project.Engine;
using PubDoomer.Project.IWad;
using PubDoomer.Saving;
using PubDoomer.Services;
using PubDoomer.ViewModels.Pages;

namespace PubDoomer.ViewModels.Dialogues;

public partial class ConfigureRunMapViewModel : PageViewModel
{
    /// <summary>
    /// Maps the engine types to a configuration type.
    /// </summary>
    private static Dictionary<EngineType, Type> _engineTypeToConfigurationMap = new()
    {
        [EngineType.Zdoom] = typeof(ZdoomEngineRunConfiguration),
        [EngineType.GzDoom] = typeof(GzDoomEngineRunConfiguration),
        [EngineType.Zandronum] = typeof(ZandronumEngineRunConfiguration),
    };
    
    /// <summary>
    /// Caches engine configurations for specific engine contexts, to be reused should the user return to a previously
    /// created instance. Also maintains the state.
    /// <br /> This dictionary is static so the states are also persisted, should this view model be removed.
    /// </summary>
    private static readonly Dictionary<EngineContext, EngineRunConfiguration> EngineRunContextCache = new();
    
    // Dependencies
    private readonly DialogueProvider? _dialogueProvider;
    
    /// <summary>
    /// The collection of Engine instances that can be selected.
    /// </summary>
    [ObservableProperty] private ObservableCollection<EngineContext> _selectableEngines;
    
    /// <summary>
    /// The collection of IWad instances that can be selected.
    /// </summary>
    [ObservableProperty] private ObservableCollection<IWadContext> _selectableIWads;
    
    /// <summary>
    /// The IWad configured to be opened.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormIsValid))]
    private IWadContext? _selectedIWad;
    
    /// <summary>
    /// The Engine configured to be opened.
    /// <br /> This property is not used by itself. Instead <see cref="SelectedEngineRunConfiguration"/> contains the full context, which is created when this property gets set.
    /// </summary>
    [ObservableProperty] private EngineContext? _selectedEngine;
    
    /// <summary>
    /// Contains the full context of the engine configured to be opened.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormIsValid))]
    private EngineRunConfiguration? _selectedEngineRunConfiguration;
    
    public ConfigureRunMapViewModel()
    {
        if (!Design.IsDesignMode) throw new InvalidOperationException();

        // Design-time data.
        var project = new CurrentProjectProvider().ProjectContext!;
        SelectableIWads = project.IWads;
        SelectableEngines = project.Engines;
    }
    
    public ConfigureRunMapViewModel(
        DialogueProvider dialogueProvider,
        IEnumerable<EngineContext> engineContexts,
        IEnumerable<IWadContext> iWadContexts,
        IWadContext? selectedIWad,
        EngineRunConfiguration? selectedEngineRunConfiguration)
    {
        _dialogueProvider = dialogueProvider;
        SelectableEngines = new ObservableCollection<EngineContext>(engineContexts);
        SelectableIWads = new ObservableCollection<IWadContext>(iWadContexts);
        SelectedEngine = selectedEngineRunConfiguration?.Context;
        SelectedEngineRunConfiguration = selectedEngineRunConfiguration;
        SelectedIWad = selectedIWad;
    }

    public bool FormIsValid => SelectedIWad != null && SelectedEngine != null && SelectedEngineRunConfiguration != null;

    partial void OnSelectedEngineChanged(EngineContext? value)
    {
        // Switch to async context.
        _ = OnSelectedEngineChangedAsync(value);
    }
    
    private async ValueTask OnSelectedEngineChangedAsync(EngineContext? value)
    {
        // Value was unset.
        if (value == null)
        {
            SelectedEngineRunConfiguration = null;
            return;
        }
        
        // Ignore engines that do not have a type defined.
        if (value.Type == EngineType.Unknown)
        {
            // Also unset the current engine.
            SelectedEngine = null;
            
            // Skip dialogue in design mode.
            if (!AssertInDesignMode())
            {
                await _dialogueProvider.AlertAsync(AlertType.Warning, "Failed to switch engine",
                    $"Failed to switch engine to '{value.Name}': the engine you selected does not have a type defined.");
            }

            return;
        }
        
        // Get cached instance if this context was previously selected.
        if (!EngineRunContextCache.TryGetValue(value, out var engineRunConfiguration))
        {
            // Create the configuration instance.
            if (!_engineTypeToConfigurationMap.TryGetValue(value.Type, out var configurationType))
            {
                Debug.Fail($"Could not find configuration for '{value.Type}'.");
                return;
            }

            if (Activator.CreateInstance(configurationType) is not EngineRunConfiguration createdEngineInstance)
            {
                Debug.Fail($"Could not create configuration for '{value.Type}'.");
                return;
            }

            EngineRunContextCache.Add(value, createdEngineInstance);
            engineRunConfiguration = createdEngineInstance;
        }

        engineRunConfiguration.Context = value;
        SelectedEngineRunConfiguration = engineRunConfiguration;
    }

    [MemberNotNullWhen(false, nameof(_dialogueProvider))]
    private bool AssertInDesignMode()
    {
        return Design.IsDesignMode;
    }
}