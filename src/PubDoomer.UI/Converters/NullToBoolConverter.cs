using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace PubDoomer.Converters;

/// <summary>
/// Represents a converter that converts a null value to a boolean.
/// If the value is <c>null</c>, the converter returns <c>true</c>.
/// Optionally, a custom value can be passed as the boolean to return if the value is <c>null</c>.
/// </summary>
public sealed class NullToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var onNull = false;

        if (parameter is bool boolValue)
        {
            onNull = boolValue;
        }
        else if (parameter is string stringValue
                 && bool.TryParse(stringValue, out boolValue))
        {
            onNull = boolValue;
        }

        return value == null ? onNull : !onNull;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}