using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Project;
using PubDoomer.Project.IWad;
using PubDoomer.Saving;
using PubDoomer.ViewModels.Pages;

namespace PubDoomer.ViewModels.Dialogues;

public partial class ConfigureEditMapViewModel : PageViewModel
{
    /// <summary>
    /// The configured path to Ultimate Doombuilder.
    /// </summary>
    [ObservableProperty] private string _udbExecutableFilePath;
    
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
    
    public ConfigureEditMapViewModel()
    {
        if (!Design.IsDesignMode) throw new InvalidOperationException();

        // Design-time data.
        _udbExecutableFilePath = new LocalSettings().UdbExecutableFilePath!;
        SelectableIWads = new CurrentProjectProvider().ProjectContext!.IWads;
    }
    
    public ConfigureEditMapViewModel(
        string udbExecutableFilePath,
        IEnumerable<IWadContext> iWadContexts)
    {
        UdbExecutableFilePath = udbExecutableFilePath;
        SelectableIWads = new ObservableCollection<IWadContext>(iWadContexts);
    }

    public bool FormIsValid => SelectedIWad != null;
}