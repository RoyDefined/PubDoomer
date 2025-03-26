using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PubDoomer.Saving;

public partial class SessionSettings : ObservableObject
{
    // If true, the application shows edit features.
    // This is very loosely implemented and if a screen is already open this doesn't really do anything.
    // This defaults to `true` when in design mode.
    // TODO: This could be improved but it's not meant to be super strict.
    [ObservableProperty] private bool _enableEditing = Design.IsDesignMode;
}