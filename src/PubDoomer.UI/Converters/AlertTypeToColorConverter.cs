using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using PubDoomer.ViewModels.Dialogues;

namespace PubDoomer.Converters;

public sealed class AlertTypeToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not AlertType type) return null;
        
        if (Application.Current == null) return null;
        
        return type switch
        {
            AlertType.None => Application.Current.FindResource("SemiColorTertiary") as SolidColorBrush,
            AlertType.Information => Application.Current.FindResource("SemiColorInformation") as SolidColorBrush,
            AlertType.Success => Application.Current.FindResource("SemiColorSuccess") as SolidColorBrush,
            AlertType.Warning => Application.Current.FindResource("SemiColorWarning") as SolidColorBrush,
            AlertType.Error => Application.Current.FindResource("SemiColorDanger") as SolidColorBrush,
            _ => Application.Current.FindResource("SemiColorTertiary") as SolidColorBrush
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}