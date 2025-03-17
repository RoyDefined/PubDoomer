using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using PubDoomer.ViewModels.Dialogues;

namespace PubDoomer.Views.Dialogues;

public partial class AddMapsWindow : Window
{
    public AddMapsWindow()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        var viewModel = (AddMapsWindowViewModel)DataContext!;
        viewModel.OnLoadAddMapsWithFileSelect();
        
        base.OnLoaded(e);
    }

    private void CancelFormButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }

    private async void FinishFormButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var viewModel = (AddMapsWindowViewModel)DataContext!;
        if (!viewModel.FormIsValid) return;

        // We add a yield so the command is able to trigger in time. Otherwise it will not trigger.
        await Task.Yield();

        Close(true);
    }
}