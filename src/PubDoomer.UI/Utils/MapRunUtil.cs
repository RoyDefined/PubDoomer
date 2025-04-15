using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PubDoomer.Engine;
using PubDoomer.Project.Archive;
using PubDoomer.Project.Engine;
using PubDoomer.Project.IWad;
using PubDoomer.Project.Maps;

namespace PubDoomer.Utils;

internal static class MapRunUtil
{
    public static void RunMap(MapContext map, EngineRunConfiguration selectedEngineRunConfiguration, IWadContext selectedIWad, IEnumerable<ArchiveContext> archives)
    {
        var argumentBuilder = new StringBuilder();

        // Include IWAD
        argumentBuilder.AppendFormat("-iwad \"{0}\" ", selectedIWad.Path);

        // Include archives
        foreach (var archive in archives)
        {
            if (string.IsNullOrEmpty(archive.Path)) continue;
            if (archive.ExcludeFromTesting) continue;
            argumentBuilder.AppendFormat("-file \"{0}\" ", archive.Path);
        }

        // Add map information
        argumentBuilder.AppendFormat("-file \"{0}\" ", map.Path);
        argumentBuilder.AppendFormat("+map {0} ", map.MapLumpName);
        
        // Add additional command line arguments for engine specific settings.
        argumentBuilder.AppendJoin(" ", selectedEngineRunConfiguration.GetCommandLineArguments());

        var processStartInfo = new ProcessStartInfo
        {
            FileName = selectedEngineRunConfiguration.Context.Path,
            Arguments = argumentBuilder.ToString(),
            UseShellExecute = false
        };

        // Start the process without waiting for it to finish
        Process.Start(processStartInfo);
    }
}