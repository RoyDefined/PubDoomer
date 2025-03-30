using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;

namespace PubDoomer.Converters.Multi;

/// <summary>
/// Represents a converter that returns <c>true</c> if all values are non-null.
/// </summary>
public sealed class NullToBoolMultiValueConverter : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        return values.All(x => x != null);
    }
}