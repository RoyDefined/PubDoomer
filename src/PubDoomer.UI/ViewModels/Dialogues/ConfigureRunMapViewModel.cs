using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Project;
using PubDoomer.Project.Engine;
using PubDoomer.Project.IWad;
using PubDoomer.Saving;
using PubDoomer.ViewModels.Pages;

namespace PubDoomer.ViewModels.Dialogues;

public partial class ConfigureRunMapViewModel : PageViewModel
{
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
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormIsValid))]
    private EngineContext? _selectedEngine;
    
    public ConfigureRunMapViewModel()
    {
        if (!Design.IsDesignMode) throw new InvalidOperationException();

        // Design-time data.
        var project = new CurrentProjectProvider().ProjectContext!;
        SelectableIWads = project.IWads;
        SelectableEngines = project.Engines;
    }
    
    public ConfigureRunMapViewModel(
        IEnumerable<EngineContext> engineContexts,
        IEnumerable<IWadContext> iWadContexts,
        IWadContext? selectedIWad,
        EngineContext? selectedEngine)
    {
        SelectableEngines = new ObservableCollection<EngineContext>(engineContexts);
        SelectableIWads = new ObservableCollection<IWadContext>(iWadContexts);
        SelectedEngine = selectedEngine;
        SelectedIWad = selectedIWad;
    }

    public bool FormIsValid => SelectedIWad != null && SelectedEngine != null;
}