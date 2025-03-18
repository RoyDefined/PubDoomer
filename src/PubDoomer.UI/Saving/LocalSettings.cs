using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Project.IWad;

namespace PubDoomer.Saving;

// TODO: Add design-time context with fake paths.
public partial class LocalSettings : ObservableObject
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
    /// Configurable locations of IWad files.
    /// </summary>
    [ObservableProperty] private ObservableCollection<IWadContext> _iWads = [];
}