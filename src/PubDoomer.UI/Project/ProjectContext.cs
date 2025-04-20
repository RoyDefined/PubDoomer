using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text.Json.Serialization;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Project.Archive;
using PubDoomer.Project.Engine;
using PubDoomer.Project.IWad;
using PubDoomer.Project.Maps;
using PubDoomer.Project.Profile;
using PubDoomer.Project.Tasks;

namespace PubDoomer.Project;

/// <summary>
/// Represents the project context that contains the full state of a project.
/// </summary>
public partial class ProjectContext : ObservableObject
{
    public const string ProjectBinaryFormatExtension = "pdbproj";
    public const string ProjectTextFormatExtension = "pdtproj";
    
    // Configuration for tasks and other features in a project.
    [ObservableProperty] private AvaloniaDictionary<string, string> _configurations = new();

    /// <summary>
    /// Represents the folder path that the project is saved under.
    /// </summary>
    [ObservableProperty] private string _folderPath = null!;
    
    /// <summary>
    /// Represents the file name that the project is saved under.
    /// </summary>
    [ObservableProperty] private string _fileName = null!;

    /// <summary>
    /// The last save type used to save the project. Defaults to binary.
    /// </summary>
    [ObservableProperty] private ProjectSaveType _saveType = ProjectSaveType.Binary;

    /// <summary>
    /// A name to give to this project to show in the UI.
    /// </summary>
    [ObservableProperty] private string _name = null!;
    
    /// <summary>
    /// Configurable profiles that contain tasks to be invoked in order.
    /// </summary>
    [ObservableProperty] private ObservableCollection<ProfileContext> _profiles = [];
    
    /// <summary>
    /// Configurable tasks that can be invoked.
    /// </summary>
    [ObservableProperty] private ObservableCollection<ProjectTaskBase> _tasks = [];
    
    /// <summary>
    /// Configurable locations of IWAD files.
    /// </summary>
    [ObservableProperty] private ObservableCollection<IWadContext> _iWads = [];
    
    /// <summary>
    /// Configurable locations of archives.
    /// </summary>
    [ObservableProperty] private ObservableCollection<ArchiveContext> _archives = [];
    
    /// <summary>
    /// Configurable locations of game engines.
    /// </summary>
    [ObservableProperty] private ObservableCollection<EngineContext> _engines = [];
    
    /// <summary>
    /// Configurable locations of maps.
    /// </summary>
    [ObservableProperty] private ObservableCollection<MapContext> _maps = [];

    public string GetFullPath()
    {
        // Determine the write type based on the last save type.
        var extension = SaveType switch
        {
            ProjectSaveType.Binary => ProjectBinaryFormatExtension,
            ProjectSaveType.Text => ProjectTextFormatExtension,
            _ => throw new ArgumentException($"Project extension not found from type: {SaveType}"),
        };
        
        return Path.Combine(FolderPath, FileName) + $".{extension}";
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