using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PubDoomer.ViewModels.Dialogues;

public partial class InformationalWindowViewModel : ViewModelBase
{
    [ObservableProperty] private string? _windowTitle;
    [ObservableProperty] private string? _title;
    [ObservableProperty] private string? _subTitle;
    [ObservableProperty] private Exception? _exception;
    [ObservableProperty] private AlertType _windowType = AlertType.None;
    [ObservableProperty] private ObservableCollection<InformationalWindowButton> _buttons = new();

    public InformationalWindowViewModel()
    {
        if (!Design.IsDesignMode) return;

        WindowTitle = string.Empty;
        WindowType = AlertType.Warning;
        Title = "Something went wrong.";
        SubTitle = "Are you sure you want to continue? This process is irreversible.";
        Exception = new Exception("This is an exception.");
        Buttons =
        [
            new InformationalWindowButton(AlertType.None, "Cancel"),
            new InformationalWindowButton(AlertType.Error, "Continue")
        ];
    }
}