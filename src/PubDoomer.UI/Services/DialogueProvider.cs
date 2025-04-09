using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using PubDoomer.ViewModels.Dialogues;
using PubDoomer.Views.Dialogues;

namespace PubDoomer.Services;

public sealed class DialogueProvider(
    WindowProvider windowProvider)
{
    // TODO: Get rid of this
    private static readonly Dictionary<Type, Type> ViewModelToWindowMap = new()
    {
        [typeof(CreateOrEditTaskWindowViewModel)] = typeof(CreateOrEditTaskWindow),
        [typeof(CreateOrEditProfileWindowViewModel)] = typeof(CreateOrEditProfileWindow),
        [typeof(CreateOrEditProjectWindowViewModel)] = typeof(CreateOrEditProjectWindow),
        [typeof(AddMapsWindowViewModel)] = typeof(AddMapsWindow),
        [typeof(EditMapWindowViewModel)] = typeof(EditMapWindow),
        [typeof(ConfigureEditMapViewModel)] = typeof(ConfigureEditMapWindow),
        [typeof(ConfigureRunMapViewModel)] = typeof(ConfigureRunMapWindow)
    };

    public async Task<int> ShowWindowAsync(Action<InformationalWindowViewModel> configureWindow)
    {
        var window = windowProvider.ProvideWindow();

        var viewModel = new InformationalWindowViewModel();
        configureWindow(viewModel);

        var dialog = new InformationalWindow()
        {
            DataContext = viewModel,
        };
        return await dialog.ShowDialog<int>(window);
    }

    public async Task<bool> PromptAsync(AlertType type, string windowTitle, string title, string subTitle,
        InformationalWindowButton falseButton, InformationalWindowButton trueButton)
    {
        return await ShowWindowAsync(vm =>
        {
            vm.WindowTitle = windowTitle;
            vm.WindowType = type;
            vm.Title = title;
            vm.SubTitle = subTitle;
            vm.Buttons =
            [
                falseButton,
                trueButton
            ];
        }) == 1;
    }

    public async Task AlertAsync(AlertType type, string title, string? message = null)
    {
        _ = await ShowWindowAsync(vm =>
        {
            vm.WindowTitle = "Alert";
            vm.WindowType = type;
            vm.Title = title;
            vm.SubTitle = message;
            vm.Buttons =
            [
                new InformationalWindowButton(type, "Continue")
            ];
        });
    }

    // Represents a generic implementation that creates a create/edit window using the given view model.
    public async Task<bool> GetCreateOrEditDialogueWindowAsync<TViewModel>(TViewModel viewModel)
    {
        if (!ViewModelToWindowMap.TryGetValue(typeof(TViewModel), out var windowType))
            throw new ArgumentException($"Failed to retrieve dialogue window for {typeof(TViewModel)}");

        var window = windowProvider.ProvideWindow();
        var windowDialogue = (Window)Activator.CreateInstance(windowType)!;
        windowDialogue.DataContext = viewModel;
        return await windowDialogue.ShowDialog<bool>(window);
    }
}