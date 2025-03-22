using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace PubDoomer.Project.Engine;

public class EngineTypeToImagePathConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not EngineType engineType)
        {
            return AvaloniaProperty.UnsetValue;
        }
        
        var path = engineType switch
        {
            EngineType.Zandronum => "/Assets/engine-zandronum.png",
            EngineType.Zdoom => "/Assets/engine-zdoom.png",
            EngineType.GzDoom => "/Assets/engine-gzdoom.png",
            
            // TODO: Provide a better image for unknown images.
            _ => "/Assets/engine-zandronum.png"
        };

        // TODO: Improve to not contain the assembly name directly.
        return new Bitmap(AssetLoader.Open(new Uri($"avares://PubDoomer.UI{path}")));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}