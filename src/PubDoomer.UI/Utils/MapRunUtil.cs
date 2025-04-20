using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PubDoomer.Engine;
using PubDoomer.Project;
using PubDoomer.Project.Archive;
using PubDoomer.Project.Engine;
using PubDoomer.Project.IWad;
using PubDoomer.Project.Maps;

namespace PubDoomer.Utils;

internal static class MapRunUtil
{
    public static void RunMap(MapContext map, ProjectContext project, EngineRunConfiguration selectedEngineRunConfiguration, IWadContext selectedIWad, IEnumerable<ArchiveContext> archives)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(selectedIWad.Path);
        ArgumentException.ThrowIfNullOrWhiteSpace(selectedEngineRunConfiguration.Context.Path);

        var argumentBuilder = new StringBuilder();

        // Include IWAD
        argumentBuilder.AppendFormat("-iwad \"{0}\" ", GetPath(project, selectedIWad.Path));

        // Include archives
        foreach (var archive in archives)
        {
            if (string.IsNullOrEmpty(archive.Path)) continue;
            if (archive.ExcludeFromTesting) continue;
            argumentBuilder.AppendFormat("-file \"{0}\" ", GetPath(project, archive.Path));
        }

        // Add map information
        argumentBuilder.AppendFormat("-file \"{0}\" ", map.Path);
        argumentBuilder.AppendFormat("+map {0} ", map.MapLumpName);
        
        // Add additional command line arguments for engine specific settings.
        argumentBuilder.AppendJoin(" ", selectedEngineRunConfiguration.GetCommandLineArguments());

        var processStartInfo = new ProcessStartInfo
        {
            FileName = GetPath(project, selectedEngineRunConfiguration.Context.Path),
            Arguments = argumentBuilder.ToString(),
            UseShellExecute = false
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
}