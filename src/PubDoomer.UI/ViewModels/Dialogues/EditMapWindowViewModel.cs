using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PubDoomer.Project;
using PubDoomer.Project.Maps;
using PubDoomer.Project.Profile;
using PubDoomer.Project.Tasks;
using PubDoomer.Services;
using IOPath = System.IO.Path;

namespace PubDoomer.ViewModels.Dialogues;

public partial class EditMapWindowViewModel : ViewModelBase
{
    // The map being edited
    [ObservableProperty] private MapContext _mapContext;
    
    public EditMapWindowViewModel()
    {
        if (!Design.IsDesignMode) throw new InvalidOperationException();

        // In design mode we add a test map
        // We get this from the project provider test data.
        MapContext = new CurrentProjectProvider().ProjectContext!.Maps.First();
    }
    
    public EditMapWindowViewModel(
        MapContext mapContext)
    {
        MapContext = mapContext;
    }

    /// <summary>
    /// Returns if the form is valid.
    /// <br />The form is valid if there's at least one map and all the maps being added contain
    /// </summary>
    public bool FormIsValid => !string.IsNullOrWhiteSpace(MapContext.Name)
                               && !string.IsNullOrWhiteSpace(MapContext.MapLumpName)
                               && !string.IsNullOrWhiteSpace(MapContext.Path);
}