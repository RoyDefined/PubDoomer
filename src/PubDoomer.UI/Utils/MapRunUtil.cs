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

internal static class MapRunUtil
{
    // TODO: Configurable log file
    // TODO: Configure bots
    // TODO: Configure multiplayer
    public static void RunMap(string engineFilePath, MapContext map, IWadContext selectedIWad, IEnumerable<ArchiveContext> archives)
    {
        var argumentBuilder = new StringBuilder();

        // Include IWAD
        argumentBuilder.AppendFormat("-iwad \"{0}\" ", selectedIWad.Path);

        // Include archives
        foreach (var archive in archives)
        {
            if (string.IsNullOrEmpty(archive.Path)) continue;
            argumentBuilder.AppendFormat("-file \"{0}\" ", archive.Path);
        }

        // Set the skill level to 4 by default.
        // TODO: Configurable.
        argumentBuilder.Append("-skill 4 ");

        // Add map information
        argumentBuilder.AppendFormat("+map {0} ", map.MapLumpName);

        var processStartInfo = new ProcessStartInfo
        {
            FileName = engineFilePath,
            Arguments = argumentBuilder.ToString(),
            UseShellExecute = false
        };

        // Start the process without waiting for it to finish
        Process.Start(processStartInfo);
    }
}