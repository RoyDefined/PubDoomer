using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Project.Run;

namespace PubDoomer.Project.Profile;

/// <summary>
/// Represents a configured profile containing tasks to execute.
/// </summary>
public partial class ProfileContext : ObservableObject, ICloneable
{
    /// <summary>
    /// The name of the profile to display in the UI.
    /// </summary>
    [ObservableProperty] private string? _name;
    
    /// <summary>
    /// A collection of observable tasks to be invoked on this profile.
    /// </summary>
    [ObservableProperty] private ObservableCollection<ProfileTask> _tasks;

    // Default constructor.
    public ProfileContext()
    {
        _tasks = new ObservableCollection<ProfileTask>();
    }

    // Cloned instance.
    public ProfileContext(string? name, ProfileTask[] tasks)
    {
        _name = name;
        _tasks = new ObservableCollection<ProfileTask>(tasks);
    }

    /// <inheritdoc/>
    public object Clone()
    {
        return DeepClone();
    }

    /// <summary>
    /// Converts the profile to a run context that is set up so all tasks contain a context specific for invokation.
    /// </summary>
    public ProfileRunContext ToProfileRunContext()
    {
        Debug.Assert(Name != null);
        return new ProfileRunContext(Name, Tasks.Select(x => x.ToRunnableTask()));
    }

    /// <summary>
    /// Performs a full clone of the profile and creates a new instance for the profile and the tasks.
    /// <br /> By design the tasks retain their reference.
    /// </summary>
    public ProfileContext DeepClone()
    {
        return new ProfileContext(Name, Tasks.ToArray());
    }
}