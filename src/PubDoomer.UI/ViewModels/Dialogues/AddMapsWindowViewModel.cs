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

public partial class AddMapsWindowViewModel : ViewModelBase
{
    private readonly WindowProvider? _windowProvider;

    // Holds the maps to be added.
    [ObservableProperty] private ObservableCollection<MapContext> _mapsToAdd = [];
    
    public AddMapsWindowViewModel()
    {
        if (!Design.IsDesignMode) throw new InvalidOperationException();

        // In design mode we add test data.
        // We get these from the project provider test data.
        MapsToAdd = new CurrentProjectProvider().ProjectContext!.Maps;
    }
    
    public AddMapsWindowViewModel(
        WindowProvider windowProvider)
    {
        _windowProvider = windowProvider;
    }

    /// <summary>
    /// Returns if the form is valid.
    /// <br />The form is valid if there's at least one map and all the maps being added contain
    /// </summary>
    public bool FormIsValid => MapsToAdd.Count != 0 && MapsToAdd.All(x =>
                                   !string.IsNullOrWhiteSpace(x.Name)
                                   && !string.IsNullOrWhiteSpace(x.MapLumpName)
                                   && !string.IsNullOrWhiteSpace(x.Path));

    /// <summary>
    /// General method to trigger when a map is updated.
    /// </summary>
    private void UpdateFormValid(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(FormIsValid));
    }

    /// <summary>
    /// The <see cref="AddMapsWithFileSelectAsync"/> equivelant for when the form is loaded.
    /// </summary>
    public void OnLoadAddMapsWithFileSelect()
    {
        _ = Task.Run(AddMapsWithFileSelectAsync);
    }

    /// <summary>
    /// Asynchronously adds maps through file selection.
    /// <br /> Used in both the UI as a command but also called directly on load.
    /// <br /> <see cref="OnLoadAddMapsWithFileSelect"/> covers file selection when the form is loaded.
    /// </summary>
    [RelayCommand]
    private async Task AddMapsWithFileSelectAsync()
    {
        if (AssertInDesignMode()) return;
        
        var window = _windowProvider.ProvideWindow();
        var filesPicker = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select one or more maps to add",
            AllowMultiple = true,
            FileTypeFilter = [
                new FilePickerFileType("WAD file") { Patterns = ["*.wad"] },
            ]
        });
        
        if (filesPicker.Count == 0) return;

        foreach (var file in filesPicker)
        {
            AddMapFromPath(file.Path.LocalPath);
        }
    }

    // Adds a map to the collection from the given path.
    private void AddMapFromPath(string path)
    {
        var mapContext = new MapContext()
        {
            Path = path,
            MapLumpName = null,
            Name = IOPath.GetFileName(path),
        };

        mapContext.PropertyChanged += UpdateFormValid;
        MapsToAdd.Add(mapContext);
    }

    /// <summary>
    /// Removes a pending map.
    /// <br /> This deliberately does not prompt since it was a pending map.
    /// </summary>
    [RelayCommand]
    private void RemoveMap(MapContext mapContext)
    {
        mapContext.PropertyChanged -= UpdateFormValid;
        MapsToAdd.Remove(mapContext);
    }
    
    
    [MemberNotNullWhen(false, nameof(_windowProvider))]
    private bool AssertInDesignMode()
    {
        return Design.IsDesignMode;
    }
}