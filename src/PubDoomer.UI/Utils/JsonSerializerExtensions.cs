using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PubDoomer.Utils;

internal static class JsonSerializerExtensions
{
    internal static void Populate<T>(T target, string json)
    {
        var newData = JsonSerializer.Deserialize<T>(json);
        if (newData == null) return;

        foreach (var property in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.CanWrite)
            {
                var newValue = property.GetValue(newData);
                property.SetValue(target, newValue);
            }
        }
    }
}
