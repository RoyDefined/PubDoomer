using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;

namespace PubDoomer.Converters.Multi;

/// <summary>
/// Represents a converter that returns <c>true</c> if all values are non-null and, if a list, have a count greater than zero.
/// </summary>
public sealed class NullAndCountToBoolMultiValueConverter : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        return values.All(x => (x != null && x is not IList) || x is IList { Count: > 0 });
    }
}