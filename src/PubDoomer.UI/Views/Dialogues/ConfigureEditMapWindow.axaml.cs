using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using PubDoomer.ViewModels.Dialogues;
using PubDoomer.ViewModels.Pages;

namespace PubDoomer.Views.Dialogues;

public partial class ConfigureEditMapWindow : Window
{
    public ConfigureEditMapWindow()
    {
        InitializeComponent();
    }

    private void CancelFormButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Design.IsDesignMode) return;
        Close(false);
    }

    private void FinishFormButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Design.IsDesignMode) return;
        Close(true);
    }
}