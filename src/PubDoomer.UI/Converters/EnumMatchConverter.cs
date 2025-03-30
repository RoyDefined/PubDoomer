using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace PubDoomer.Converters;

/// <summary>
/// Represents a converter that returns <c>true</c> if the given enum value matches the given parameter name.
/// </summary>
public sealed class EnumMatchConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Enum enumValue && parameter is string targetName)
        {
            return string.Equals(enumValue.ToString(), targetName, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
