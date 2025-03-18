using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Project.Archive;
using PubDoomer.Project.Maps;
using PubDoomer.Project.Profile;
using PubDoomer.Project.Tasks;

namespace PubDoomer.Project;

/// <summary>
/// Represents the project context that contains the full state of a project.
/// </summary>
public partial class ProjectContext : ObservableObject
{
    // Configurable executions
    [ObservableProperty] private string? _accCompilerExecutableFilePath;
    [ObservableProperty] private string? _bccCompilerExecutableFilePath;
    [ObservableProperty] private string? _gdccCompilerExecutableFilePath;
    [ObservableProperty] private string? _sladeExecutableFilePath;
    [ObservableProperty] private string? _udbExecutableFilePath;
    [ObservableProperty] private string? _acsVmExecutableFilePath;
    [ObservableProperty] private string? _zandronumExecutableFilePath;

    /// <summary>
    /// Represents the file path that the project is saved under.
    /// If <c>true</c>, the project was not saved yet.
    /// </summary>
    [ObservableProperty] [property: JsonIgnore]
    private Uri? _filePath;

    /// <summary>
    /// A name to give to this project to show in the UI.
    /// </summary>
    [ObservableProperty] private string? _name;
    
    /// <summary>
    /// Configurable profiles that contain tasks to be invoked in order.
    /// </summary>
    [ObservableProperty] private ObservableCollection<ProfileContext> _profiles;
    
    /// <summary>
    /// Configurable tasks that can be invoked.
    /// </summary>
    [ObservableProperty] private ObservableCollection<ProjectTaskBase> _tasks;
    
    /// <summary>
    /// Configurable locations of archives.
    /// </summary>
    [ObservableProperty] private ObservableCollection<ArchiveContext> _archives;
    
    /// <summary>
    /// Configurable locations of maps.
    /// </summary>
    [ObservableProperty] private ObservableCollection<MapContext> _maps;

    public ProjectContext()
    {
        Tasks = [];
        Profiles = [];
        Archives = [];
        Maps = [];
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