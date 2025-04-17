using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PubDoomer.Project;
using PubDoomer.Services;

namespace PubDoomer.ViewModels.Dialogues;

public partial class CreateOrEditProjectWindowViewModel : ViewModelBase
{
    private readonly WindowProvider? _windowProvider;

    [ObservableProperty] private string _createOrEditButtonText;

    // The currently project.
    [ObservableProperty] private ProjectContext _project;

    // Form visuals
    [ObservableProperty] private string _windowTitle;
    
    public CreateOrEditProjectWindowViewModel()
    {
        if (!Design.IsDesignMode) throw new InvalidOperationException();
        
        Project = new ProjectContext();

        WindowTitle = "Create new Project";
        CreateOrEditButtonText = "Create";

        SubscribeProjectChanges();
    }

    public CreateOrEditProjectWindowViewModel(
        WindowProvider windowProvider)
    {
        _windowProvider = windowProvider;
        Project = new ProjectContext();

        WindowTitle = "Create new Project";
        CreateOrEditButtonText = "Create";

        SubscribeProjectChanges();
    }

    public CreateOrEditProjectWindowViewModel(
        WindowProvider windowProvider,
        ProjectContext project)
    {
        _windowProvider = windowProvider;
        Project = project;

        WindowTitle = "Edit Project";
        CreateOrEditButtonText = "Edit";

        SubscribeProjectChanges();
    }

    public bool FormIsValid => !string.IsNullOrWhiteSpace(Project.Name)
                               && !string.IsNullOrWhiteSpace(Project.FolderPath)
                               && !string.IsNullOrWhiteSpace(Project.FileName);
    
    [RelayCommand]
    private async Task PickProjectPathAsync()
    {
        if (AssertInDesignMode()) return;

        var window = _windowProvider.ProvideWindow();
        var folderPicker = await window.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            Title = "Select project location",
            AllowMultiple = false
        });

        if (folderPicker.Count == 0) return;

        var folderPath = folderPicker.First().Path.AbsolutePath;
        Project.FolderPath = folderPath;
    }

    private void SubscribeProjectChanges()
    {
        Project.PropertyChanged += UpdateFormValid;
    }

    private void UpdateFormValid(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(FormIsValid));
    }
    
    [MemberNotNullWhen(false, nameof(_windowProvider))]
    private bool AssertInDesignMode()
    {
        return Design.IsDesignMode;
    }
}