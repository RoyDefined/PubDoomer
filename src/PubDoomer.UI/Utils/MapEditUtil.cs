using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PubDoomer.Project.Archive;
using PubDoomer.Project.IWad;
using PubDoomer.Project.Maps;

namespace PubDoomer.Utils;

internal static class MapEditUtil
{
    private const string UdbConfigurationFolderPath = "Configurations";
    
    /// <summary>
    /// Starts Ultimate Doombuilder using the provided filepath, opening the given map and optionally loading one or more archives.
    /// <br /> The method will ensure the process is started in the background and doesn't get awaited.
    /// </summary>
    internal static void StartUltimateDoomBuilder(string filePath, MapContext map, IWadContext selectedIWad,
        string selectedConfiguration, IEnumerable<ArchiveContext> archives)
    {
        var argumentBuilder = new StringBuilder();
            
        // Add the main WAD file (the map file) and map lump name
        argumentBuilder.AppendFormat("\"{0}\" -map {1}", map.Path, map.MapLumpName);
        
        // Add configuration
        argumentBuilder.Append($" -cfg \"{selectedConfiguration}.cfg\"");
        
        // Add IWad
        argumentBuilder.AppendFormat(" -resource wad \"{0}\"", selectedIWad.Path);
            
        // Add each archive as a resource
        foreach (var archive in archives)
        {
            if (string.IsNullOrEmpty(archive.Path)) continue;

            // TODO: Determine if the file is a `wad`, `pk3` or `dir`
            var excludeFlag = archive.ExcludeFromTesting ? "notest " : string.Empty;
            argumentBuilder.AppendFormat(" -resource dir {0}\"{1}\"", excludeFlag, archive.Path);
        }

        var processStartInfo = new ProcessStartInfo
        {
            FileName = filePath,
            Arguments = argumentBuilder.ToString(),
            UseShellExecute = false,
        };

        // Start the process without waiting for it to finish
        Process.Start(processStartInfo);
    }

    // TODO: Support additional folders, but not the '/Included' folder.
    public static IEnumerable<string> GetConfigurations(string udbExecutableFilePath)
    {
        var path = Path.GetDirectoryName(udbExecutableFilePath) ?? string.Empty;
        path = Path.Combine(path, UdbConfigurationFolderPath);
        if (!Directory.Exists(path))
        {
            throw new DirectoryNotFoundException($"The directory '{path}' was not found.");
        }

        return Directory.GetFiles(path, "*.cfg", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileNameWithoutExtension)
            .OfType<string>();
    }

    internal static void StartSlade(string filePath, MapContext map)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = filePath,
            Arguments = map.Path,
            UseShellExecute = false,
        };

        // Start the process without waiting for it to finish
        Process.Start(processStartInfo);
    }
}