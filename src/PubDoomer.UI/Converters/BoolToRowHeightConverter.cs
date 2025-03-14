using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace PubDoomer.Converters;

public sealed class BoolToRowHeightConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool boolValue) return GridLength.Auto;

        if (parameter is not string stringValue) return GridLength.Auto;

        // Bool value is `false` so return zero length.
        if (!boolValue) return new GridLength(0);

        // Handle specific cases like `Auto` and `*`.
        // Apart from that the length must be parsable, otherwise default to `Auto`.
        return stringValue switch
        {
            "Auto" => GridLength.Auto,
            "*" => GridLength.Star,
            _ when int.TryParse(stringValue, out var intValue) => new GridLength(intValue),
            _ => GridLength.Auto
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}