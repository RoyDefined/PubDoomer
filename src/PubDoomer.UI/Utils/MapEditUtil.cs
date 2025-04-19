using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PubDoomer.Engine;
using PubDoomer.Project;
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
    internal static void StartUltimateDoomBuilder(string filePath, MapContext map, ProjectContext projectContext, IWadContext selectedIWad,
        string selectedConfiguration, IEnumerable<ArchiveContext> archives)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(map.Path);
        ArgumentException.ThrowIfNullOrWhiteSpace(selectedIWad.Path);

        var argumentBuilder = new StringBuilder();
            
        // Add the main WAD file (the map file) and map lump name
        argumentBuilder.AppendFormat("\"{0}\" -map {1}", GetPath(projectContext, map.Path), map.MapLumpName);
        
        // Add configuration
        argumentBuilder.Append($" -cfg \"{selectedConfiguration}.cfg\"");
        
        // Add IWad
        argumentBuilder.AppendFormat(" -resource wad \"{0}\"", GetPath(projectContext, selectedIWad.Path));
            
        // Add each archive as a resource
        foreach (var archive in archives)
        {
            if (string.IsNullOrEmpty(archive.Path)) continue;

            var excludeFlag = archive.ExcludeFromTesting ? "notest " : string.Empty;

            // TODO: Determine if the file is a `wad`, `pk3` or `dir`
            argumentBuilder.AppendFormat(" -resource dir {0}\"{1}\"",
                excludeFlag, GetPath(projectContext, archive.Path));
        }

        var processStartInfo = new ProcessStartInfo
        {
            FileName = GetPath(projectContext, filePath),
            Arguments = argumentBuilder.ToString(),
            UseShellExecute = false,
        };

        // Start the process without waiting for it to finish
        Process.Start(processStartInfo);
    }

    internal static void StartSlade(string filePath, IEnumerable<string> paths, ProjectContext projectContext)
    {
        var arguments = string.Join(" ", paths.Select(x => $"\"{GetPath(projectContext, x)}\""));
        var processStartInfo = new ProcessStartInfo
        {
            FileName = GetPath(projectContext, filePath),
            Arguments = arguments,
            UseShellExecute = false,
        };

        // Start the process without waiting for it to finish
        Process.Start(processStartInfo);
    }

    private static string GetPath(ProjectContext project, string filePath)
    {
        if (Path.IsPathRooted(filePath))
            return filePath;

        // Handle relative input path
        if (string.IsNullOrWhiteSpace(project.FolderPath))
            throw new ArgumentException($"Failed to update relative input path ({filePath}). No project directory was specified. Either the project directory must be specified or the path must be absolute.");

        return Path.Combine(project.FolderPath, filePath);
    }

    // TODO: Support additional folders, but not the '/Included' folder.
    public static IEnumerable<string> GetConfigurations(ProjectContext projectContext, string udbExecutableFilePath)
    {
        var path = Path.GetDirectoryName(GetPath(projectContext, udbExecutableFilePath)) ?? string.Empty;
        path = Path.Combine(path, UdbConfigurationFolderPath);
        if (!Directory.Exists(path))
        {
            throw new DirectoryNotFoundException($"The directory '{path}' was not found.");
        }

        return Directory.GetFiles(path, "*.cfg", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileNameWithoutExtension)
            .OfType<string>();
    }
}