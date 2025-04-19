using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using PubDoomer.ViewModels.Dialogues;

namespace PubDoomer.Views.Dialogues;

public partial class CreateOrEditTaskWindow : Window
{
    public CreateOrEditTaskWindow()
    {
        InitializeComponent();
    }

    private void FormButtonCanceled_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Design.IsDesignMode) return;

        Close(false);
    }

    private void FormButtonSuccess_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Design.IsDesignMode) return;

        var viewModel = (CreateOrEditTaskWindowViewModel)DataContext!;
        if (!viewModel.FormIsValid) return;

        viewModel.OnSaveTask();
        Close(true);
    }
}