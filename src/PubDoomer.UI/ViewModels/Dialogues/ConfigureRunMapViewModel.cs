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

public partial class ConfigureRunMapViewModel : PageViewModel
{
    /// <summary>
    /// The configured path to the engine.
    /// </summary>
    [ObservableProperty] private string _zandronumExecutableFilePath;
    
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
    
    public ConfigureRunMapViewModel()
    {
        if (!Design.IsDesignMode) throw new InvalidOperationException();

        // Design-time data.
        ZandronumExecutableFilePath = new LocalSettings().ZandronumExecutableFilePath!;
        SelectableIWads = new CurrentProjectProvider().ProjectContext!.IWads;
    }
    
    public ConfigureRunMapViewModel(
        string zandronumExecutableFilePath,
        IEnumerable<IWadContext> iWadContexts,
        IWadContext? selectedIWad)
    {
        ZandronumExecutableFilePath = zandronumExecutableFilePath;
        SelectableIWads = new ObservableCollection<IWadContext>(iWadContexts);
        SelectedIWad = selectedIWad;
    }

    public bool FormIsValid => SelectedIWad != null;
}