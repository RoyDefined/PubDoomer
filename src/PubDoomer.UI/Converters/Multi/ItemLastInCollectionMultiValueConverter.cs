using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.MarkupExtensions.CompiledBindings;

namespace PubDoomer.Converters.Multi;

/// <summary>
/// Represents a converter that returns a boolean indicating if the given item is the last item in a collection.
/// <br/> The item must be a reference type that is able to compare by reference against other items in the collection.
/// </summary>
public sealed class ItemLastInCollectionMultiValueConverter : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count != 2) return false;
        
        var value = values[0];
        var list = values[1] as IList;
        
        if (list == null) return false;
        
        return list.IndexOf(value) == list.Count - 1;
    }
}