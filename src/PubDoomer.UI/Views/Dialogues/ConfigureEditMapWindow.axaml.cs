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
        Close(false);
    }

    private async void FinishFormButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var viewModel = (ConfigureEditMapViewModel)DataContext!;
        if (!viewModel.FormIsValid) return;

        // We add a yield so the command is able to trigger in time. Otherwise it will not trigger.
        await Task.Yield();

        Close(true);
    }
}