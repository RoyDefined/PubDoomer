using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using PubDoomer.ViewModels.Dialogues;
using PubDoomer.ViewModels.Pages;

namespace PubDoomer.Views.Dialogues;

public partial class ConfigureRunMapWindow : Window
{
    public ConfigureRunMapWindow()
    {
        InitializeComponent();
    }

    private void CancelFormButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Design.IsDesignMode) return;

        Close(false);
    }

    private async void FinishFormButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Design.IsDesignMode) return;
        
        var viewModel = (ConfigureRunMapViewModel)DataContext!;
        if (!viewModel.FormIsValid) return;

        // We add a yield so the command is able to trigger in time. Otherwise it will not trigger.
        await Task.Yield();

        Close(true);
    }
}