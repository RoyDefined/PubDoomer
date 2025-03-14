using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace PubDoomer.Converters;

// Represents a converter that converts a null value to a boolean.
// If the value is `null`, the converter returns `true`.
// Optionally a custom value can be passed as the boolean to return, should the value be `null`.
public sealed class NullToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var onNull = false;

        if (parameter is bool boolValue)
            onNull = boolValue;
        else if (parameter is string stringValue
                 && bool.TryParse(stringValue, out boolValue))
            onNull = boolValue;

        return value == null ? onNull : !onNull;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}