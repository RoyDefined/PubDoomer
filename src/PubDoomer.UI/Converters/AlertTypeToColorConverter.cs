using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using PubDoomer.ViewModels.Dialogues;

namespace PubDoomer.Converters;

/// <summary>
/// Represents a converter that converts a given <see cref="AlertType"/> to a color brush.
/// </summary>
public sealed class AlertTypeToColorConverter : IValueConverter
{
    private static readonly Dictionary<AlertType, string> TypeToResourceKey = new()
    {
        [AlertType.None] = "SemiColorTertiary",
        [AlertType.Information] = "SemiColorInformation",
        [AlertType.Success] = "SemiColorSuccess",
        [AlertType.Warning] = "SemiColorWarning",
        [AlertType.Error] = "SemiColorDanger"
    };

    private static readonly Dictionary<AlertType, SolidColorBrush?> ColorCache = new();
    
    private static SolidColorBrush? _defaultColor;
    private static SolidColorBrush? DefaultColor => _defaultColor ??= GetResource("SemiColorTertiary");

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not AlertType type) return null;
        if (Application.Current == null) return null;

        // Return previous instance if a brush was already created.
        if (!ColorCache.TryGetValue(type, out var brush))
        {
            // Try to get type, fallback to default color.
            brush = TypeToResourceKey.TryGetValue(type, out var resourceKey)
                ? GetResource(resourceKey)
                : DefaultColor;

            ColorCache[type] = brush;
        }

        return brush;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();

    private static SolidColorBrush? GetResource(string key)
    {
        Debug.Assert(Application.Current != null);
        return Application.Current.FindResource(key) as SolidColorBrush;
    }
}