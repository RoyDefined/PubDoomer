using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Project;

namespace PubDoomer.ViewModels.Dialogues;

public partial class CreateOrEditProjectWindowViewModel : ViewModelBase
{
    [ObservableProperty] private string _createOrEditButtonText;

    // The currently project.
    [ObservableProperty] private ProjectContext _project;

    // Form visuals
    [ObservableProperty] private string _windowTitle;

    public CreateOrEditProjectWindowViewModel()
    {
        Project = new ProjectContext();

        WindowTitle = "Create new Project";
        CreateOrEditButtonText = "Create";

        SubscribeProjectChanges();
    }

    public CreateOrEditProjectWindowViewModel(ProjectContext project)
    {
        Project = project;

        WindowTitle = "Edit Project";
        CreateOrEditButtonText = "Edit";

        SubscribeProjectChanges();
    }

    public bool FormIsValid => !string.IsNullOrWhiteSpace(Project.Name);

    private void SubscribeProjectChanges()
    {
        Project.PropertyChanged += UpdateFormValid;
    }

    private void UpdateFormValid(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(FormIsValid));
    }
}