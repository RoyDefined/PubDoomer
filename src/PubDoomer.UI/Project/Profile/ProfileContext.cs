using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Project.Run;

namespace PubDoomer.Project.Profile;

public partial class ProfileContext : ObservableObject, ICloneable
{
    [ObservableProperty] private string? _name;
    [ObservableProperty] private ObservableCollection<ProfileTask> _tasks;

    public ProfileContext()
    {
        _tasks = new ObservableCollection<ProfileTask>();
    }

    public ProfileContext(string? name, ProfileTask[] tasks)
    {
        _name = name;
        _tasks = new ObservableCollection<ProfileTask>(tasks);
    }

    public object Clone()
    {
        return DeepClone();
    }

    public ProfileRunContext ToProfileRunContext()
    {
        Debug.Assert(Name != null);
        return new ProfileRunContext(Name, Tasks.Select(x => x.ToRunnableTask()));
    }

    public ProfileContext DeepClone()
    {
        return new ProfileContext(Name, Tasks.ToArray());
    }
}