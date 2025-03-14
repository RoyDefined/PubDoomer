using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using PubDoomer.ViewModels.Dialogues;

namespace PubDoomer.Views.Dialogues;

public partial class InformationalWindow : Window
{
    public InformationalWindow()
    {
        InitializeComponent();
    }

    private void CloseFormButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Design.IsDesignMode) return;

        // Determine the button's index.
        if (sender is not Button { DataContext: InformationalWindowButton clickedButton })
        {
            Debug.Fail("Sender is expected to be a button with the button record.");
            return;
        }

        if (DataContext is not InformationalWindowViewModel viewModel)
        {
            Debug.Fail("The data context is expected to be a valid view model.");
            return;
        }

        // Find index of the clicked button
        var index = viewModel.Buttons.IndexOf(clickedButton);

        if (index == -1)
        {
            Debug.Fail("The clicked button is expected to be found.");
            return;
        }

        Close(index);
    }
}