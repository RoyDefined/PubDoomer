using System;
using System.Diagnostics;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace PubDoomer.Converters;

/// <summary>
/// Represents a converter that converts a bool value to a specified row height if <c>true</c>.
/// </summary>
public sealed class BoolToRowHeightConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool boolValue) return GridLength.Auto;
        if (parameter is not string stringValue) return GridLength.Auto;

        // If the bool value is false, return zero length.
        if (!boolValue) return new GridLength(0);

        // Handle specific cases like `Auto` and `*`.
        return stringValue switch
        {
            "Auto" => GridLength.Auto,
            "*" => GridLength.Star,
            _ when int.TryParse(stringValue, out var intValue) => new GridLength(intValue),
            _ => GridLength.Auto
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}