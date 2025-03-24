using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace PubDoomer.Converters;

/// <summary>
/// Represents a converter that returns <c>true</c> if the given count value is higher than 0.
/// </summary>
public sealed class CountToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int intValue) return false;
        return intValue > 0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}