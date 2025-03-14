using System.Collections.ObjectModel;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PubDoomer.ViewModels.Dialogues;

public partial class InformationalWindowViewModel : ViewModelBase
{
    [ObservableProperty] private ObservableCollection<InformationalWindowButton> _buttons = new();
    [ObservableProperty] private string? _subTitle;

    [ObservableProperty] private string? _title;
    [ObservableProperty] private string? _windowTitle;
    [ObservableProperty] private AlertType _windowType = AlertType.None;

    public InformationalWindowViewModel()
    {
        if (!Design.IsDesignMode) return;

        WindowTitle = string.Empty;
        WindowType = AlertType.Warning;
        Title = "Are you sure you want to continue?";
        SubTitle = "This process is irreversible.";
        Buttons =
        [
            new InformationalWindowButton(AlertType.None, "Cancel"),
            new InformationalWindowButton(AlertType.Error, "Continue")
        ];
    }
}