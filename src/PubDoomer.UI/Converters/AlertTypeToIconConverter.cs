using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using PubDoomer.ViewModels.Dialogues;

namespace PubDoomer.Converters;

public sealed class AlertTypeToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not AlertType type) return null;
        
        if (Application.Current == null) return null;
        
        return type switch
        {
            AlertType.None => Application.Current.FindResource("SemiIconInfoCircle") as Geometry,
            AlertType.Information => Application.Current.FindResource("SemiIconInfoCircle") as Geometry,
            AlertType.Success => Application.Current.FindResource("SemiIconCheckBoxTick") as Geometry,
            AlertType.Warning => Application.Current.FindResource("SemiIconAlertTriangle") as Geometry,
            AlertType.Error => Application.Current.FindResource("SemiIconUploadError") as Geometry,
            _ => Application.Current.FindResource("SemiIconInfoCircle") as Geometry
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}