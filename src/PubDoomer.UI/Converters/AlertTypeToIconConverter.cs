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
/// Represents a converter that converts a given <see cref="AlertType"/> to an icon.
/// </summary>
public sealed class AlertTypeToIconConverter : IValueConverter
{
    private static readonly Dictionary<AlertType, string> TypeToResourceKey = new()
    {
        [AlertType.None] = "SemiIconInfoCircle",
        [AlertType.Information] = "SemiIconInfoCircle",
        [AlertType.Success] = "SemiIconCheckBoxTick",
        [AlertType.Warning] = "SemiIconAlertTriangle",
        [AlertType.Error] = "SemiIconUploadError"
    };

    private static readonly Dictionary<AlertType, Geometry?> IconCache = new();
    
    private static Geometry? _defaultIcon;
    private static Geometry? DefaultIcon => _defaultIcon ??= GetResource("SemiIconInfoCircle");

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not AlertType type) return null;
        if (Application.Current == null) return null;

        // Return previous instance if an icon was already created.
        if (!IconCache.TryGetValue(type, out var icon))
        {
            // Try to get type, fallback to default icon.
            icon = TypeToResourceKey.TryGetValue(type, out var resourceKey)
                ? GetResource(resourceKey)
                : DefaultIcon;

            IconCache[type] = icon;
        }

        return icon;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();

    private static Geometry? GetResource(string key)
    {
        Debug.Assert(Application.Current != null);
        return Application.Current.FindResource(key) as Geometry;
    }
}
