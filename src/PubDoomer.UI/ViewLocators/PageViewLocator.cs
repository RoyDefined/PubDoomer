using System;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using PubDoomer.ViewModels;

namespace PubDoomer.ViewLocators;

public partial class PageViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param == null) return new TextBlock { Text = "Not Found: Param is null." };

        var name = param.GetType().FullName!;
        name = ViewModelRegex().Replace(name, "$1.Views.Pages.$2View");

        var type = Type.GetType(name);

        if (type == null) return new TextBlock { Text = $"Not Found: Type {name} does not exist." };

        var instance = Activator.CreateInstance(type);
        if (instance is not Control control)
            return new TextBlock { Text = $"Not Found: Type {name} is not a control." };

        control.DataContext = param;
        return control;
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }

    [GeneratedRegex(@"^(PubDoomer)\.ViewModels\.Pages\.(\w+)ViewModel$")]
    private static partial Regex ViewModelRegex();
}