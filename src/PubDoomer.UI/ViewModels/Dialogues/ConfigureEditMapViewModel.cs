using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Project;
using PubDoomer.Project.IWad;
using PubDoomer.Saving;
using PubDoomer.Utils;
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
    /// The collection of configurations that can be selected.
    /// <br /> Currently this only lists names and not anything else. UDB is aware of the configurations.
    /// </summary>
    [ObservableProperty] private ObservableCollection<string> _selectableConfigurations;
    
    /// <summary>
    /// The collection of compilers that can be selected.
    /// <br /> This list is determined the same way Ultimate Doombuilder determines compilers.
    /// </summary>
    [ObservableProperty] private ObservableCollection<UdbCompiler> _selectableCompilers;
    
    /// <summary>
    /// The IWad configured to be opened.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormIsValid))]
    private IWadContext? _selectedIWad;
    
    /// <summary>
    /// The Configuration configured to be used with the map.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormIsValid))]
    private string? _selectedConfiguration;
    
    /// <summary>
    /// The compiler configured to be used with the map.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormIsValid))]
    private UdbCompiler? _selectedCompiler;
    
    public ConfigureEditMapViewModel()
    {
        if (!Design.IsDesignMode) throw new InvalidOperationException();

        // Design-time data.
        _udbExecutableFilePath = new LocalSettings().Configurations[SavingStatics.UdbExecutableFilePathKey]!;
        SelectableIWads = new CurrentProjectProvider().ProjectContext!.IWads;
        SelectableConfigurations =
        [
            "Boom_Doom2Doom",
            "Boom_DoomDoom",
            "Doom_Doom2Doom",
            "Doom_DoomDoom",
        ];
        SelectableCompilers =
        [
            new("ACC", "ACC.cfg"),
            new("BCC", "BCC.cfg"),
            new("BCC (fork)", "ZT-BCC.cfg"),
        ];
    }
    
    public ConfigureEditMapViewModel(
        string udbExecutableFilePath,
        IEnumerable<IWadContext> iWadContexts,
        IEnumerable<string> configurations,
        IEnumerable<UdbCompiler> compilers,
        IWadContext? selectedIWad,
        string? selectedConfiguration,
        UdbCompiler selectedCompiler)
    {
        UdbExecutableFilePath = udbExecutableFilePath;
        SelectableIWads = new ObservableCollection<IWadContext>(iWadContexts);
        SelectableConfigurations = new ObservableCollection<string>(configurations);
        SelectableCompilers = new ObservableCollection<UdbCompiler>(compilers);
        SelectedIWad = selectedIWad;
        SelectedConfiguration = selectedConfiguration;
        SelectedCompiler = selectedCompiler;
    }

    public bool FormIsValid => SelectedIWad != null && SelectedConfiguration != null && SelectedCompiler != null;
}