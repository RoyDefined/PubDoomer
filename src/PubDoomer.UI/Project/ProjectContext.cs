using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Project.Profile;
using PubDoomer.Project.Tasks;

namespace PubDoomer.Project;

public partial class ProjectContext : ObservableObject
{
    // Configurable executions
    [ObservableProperty] private string? _accCompilerExecutableFilePath;
    [ObservableProperty] private string? _bccCompilerExecutableFilePath;
    [ObservableProperty] private string? _gdccCompilerExecutableFilePath;
    [ObservableProperty] private string? _sladeExecutableFilePath;
    [ObservableProperty] private string? _udbExecutableFilePath;
    [ObservableProperty] private string? _acsVmExecutableFilePath;

    /// <summary>
    /// Represents the file path that the project is saved under.
    /// If <c>true</c>, the project was not saved yet.
    /// </summary>
    [ObservableProperty] [property: JsonIgnore]
    private Uri? _filePath;


    [ObservableProperty] private string? _name;
    [ObservableProperty] private ObservableCollection<ProfileContext> _profiles;
    [ObservableProperty] private ObservableCollection<ProjectTaskBase> _tasks;

    public ProjectContext()
    {
        Tasks = [];
        Profiles = [];
    }

    public void AddTask(ProjectTaskBase task)
    {
        Tasks.Add(task);
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Tasks)));
    }

    public void RemoveTask(ProjectTaskBase task)
    {
        Tasks.Remove(task);
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Tasks)));
    }
}