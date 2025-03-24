using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Avalonia.Data.Converters;

namespace PubDoomer.Converters;

/// <summary>
/// Represents a converter that converts an enum value to its description attribute or name.
/// </summary>
public sealed class EnumDescriptionConverter : IValueConverter
{
    private static readonly ConcurrentDictionary<Enum, string> CachedDescriptions = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Enum enumValue) return value?.ToString() ?? "N/A";

        return CachedDescriptions.GetOrAdd(enumValue, GetEnumDescription);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();

    private static string GetEnumDescription(Enum enumValue)
    {
        var field = enumValue.GetType().GetField(enumValue.ToString());
        var attribute = field?.GetCustomAttribute<DescriptionAttribute>();

        return attribute?.Description ?? enumValue.ToString();
    }
}