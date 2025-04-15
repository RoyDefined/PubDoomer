using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Project;
using PubDoomer.Project.IWad;
using PubDoomer.Saving;
using PubDoomer.Utils;
using PubDoomer.ViewModels.Pages;

namespace PubDoomer.ViewModels.Dialogues;

// TODO: The combo box items should include a "none" that indicate no value was provided. Right now we can't revert a selected item.

public partial class ConfigureEditMapViewModel : PageViewModel
{
    /// <summary>
    /// The configured path to Ultimate Doombuilder.
    /// </summary>
    [ObservableProperty] private string _udbExecutableFilePath;
    
    /// <summary>
    /// The collection of IWad instances that can be selected.
    /// </summary>
    [ObservableProperty] private ObservableCollection<KeyValuePair<string, IWadContext?>> _selectableIWads;
    
    /// <summary>
    /// The collection of configurations that can be selected.
    /// <br /> Currently this only lists names and not anything else. UDB is aware of the configurations.
    /// </summary>
    [ObservableProperty] private ObservableCollection<KeyValuePair<string, string?>> _selectableConfigurations;
    
    /// <summary>
    /// The collection of compilers that can be selected.
    /// <br /> This list is determined the same way Ultimate Doombuilder determines compilers.
    /// </summary>
    [ObservableProperty] private ObservableCollection<KeyValuePair<string, UdbCompiler?>> _selectableCompilers;
    
    /// <summary>
    /// The IWad configured to be opened.
    /// </summary>
    [ObservableProperty]
    private IWadContext? _selectedIWad;
    
    /// <summary>
    /// The Configuration configured to be used with the map.
    /// </summary>
    [ObservableProperty]
    private string? _selectedConfiguration;
    
    /// <summary>
    /// The compiler configured to be used with the map.
    /// </summary>
    [ObservableProperty]
    private UdbCompiler? _selectedCompiler;
    
    public ConfigureEditMapViewModel()
    {
        if (!Design.IsDesignMode) throw new InvalidOperationException();

        // Design-time data.
        _udbExecutableFilePath = new LocalSettings().Configurations[SavingStatics.UdbExecutableFilePathKey]!;
        
        var selectableIWadsIterator = new CurrentProjectProvider().ProjectContext!.IWads.Select(x =>
            new KeyValuePair<string, IWadContext>(x.Name!, x));
        SelectableIWads =
        [
            AddNone<IWadContext>(),
            ..selectableIWadsIterator!
        ];
            
        SelectableConfigurations =
        [
            AddNone<string>(),
            new("Boom_Doom2Doom", "Boom_Doom2Doom"),
            new("Boom_DoomDoom", "Boom_DoomDoom"),
            new("Doom_Doom2Doom", "Doom_Doom2Doom"),
            new("Doom_DoomDoom", "Doom_DoomDoom"),
        ];
        
        SelectableCompilers =
        [
            AddNone<UdbCompiler>(),
            new("ACC", new("ACC", "ACC.cfg")),
            new("BCC", new("BCC", "BCC.cfg")),
            new("BCC (fork)", new("BCC (fork)", "ZT-BCC.cfg")),
        ];
    }
    
    public ConfigureEditMapViewModel(
        string udbExecutableFilePath,
        IEnumerable<IWadContext> iWadContexts,
        IEnumerable<string> configurations,
        IEnumerable<UdbCompiler> compilers,
        IWadContext? selectedIWad,
        string? selectedConfiguration,
        UdbCompiler? selectedCompiler)
    {
        UdbExecutableFilePath = udbExecutableFilePath;
        
        var selectableIWadsIterator = iWadContexts.Select(x => new KeyValuePair<string, IWadContext>(x.Name!, x));
        SelectableIWads =
        [
            AddNone<IWadContext>(),
            ..selectableIWadsIterator!
        ];
        
        var selectableConfigurationsIterator = configurations.Select(x => new KeyValuePair<string, string>(x, x));
        SelectableConfigurations = 
        [
            AddNone<string>(),
            ..selectableConfigurationsIterator!
        ];
        
        var selectableCompilersIterator = compilers.Select(x => new KeyValuePair<string, UdbCompiler>(x.Name, x));
        SelectableCompilers =
        [
            AddNone<UdbCompiler>(),
            ..selectableCompilersIterator!
        ];
        
        SelectedIWad = selectedIWad;
        SelectedConfiguration = selectedConfiguration;
        SelectedCompiler = selectedCompiler;
    }
    
    private static KeyValuePair<string, TValue?> AddNone<TValue>()
        where TValue: class => new("None", null);
}