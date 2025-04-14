using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PubDoomer.Project.Archive;
using PubDoomer.Project.IWad;
using PubDoomer.Project.Maps;

namespace PubDoomer.Utils;

internal static partial class MapEditUtil
{
    private const string UdbConfigurationFolderPath = "Configurations";
    private const string UdbScriptsFolderPath = "Scripting";

    [GeneratedRegex(@"^scripttype\s*=\s*[""]?(.*?)[""]?;", RegexOptions.IgnoreCase, "en-NL")]
    private static partial Regex UdbConfigurationScriptTypeLineRegex();

    [GeneratedRegex(@"^description\s*=\s*[""]?(.*?)[""]?;", RegexOptions.IgnoreCase, "en-NL")]
    private static partial Regex UdbConfigurationDescriptionLineRegex();

    /// <summary>
    /// Starts Ultimate Doombuilder using the provided filepath, opening the given map and optionally loading one or more archives.
    /// <br /> The method will ensure the process is started in the background and doesn't get awaited.
    /// </summary>
    internal static void StartUltimateDoomBuilder(string filePath, MapContext map,
        IWadContext? selectedIWad, string? selectedConfiguration, UdbCompiler? selectedCompiler,
        IEnumerable<ArchiveContext> archives)
    {
        var argumentBuilder = new StringBuilder();
            
        // Add the main WAD file (the map file) and map lump name
        argumentBuilder.AppendFormat("\"{0}\" -map {1}", map.Path, map.MapLumpName);
        
        // Add configuration
        if (selectedConfiguration != null)
            argumentBuilder.Append($" -cfg \"{selectedConfiguration}.cfg\"");

        // Add script configuration
        if (selectedCompiler != null)
            argumentBuilder.Append($" -scriptconfig \"{selectedCompiler.IdentifyingName}\"");
        
        // Add IWad
        if (selectedIWad != null)
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
            throw new DirectoryNotFoundException($"Failed to fetch UDB configurations. The directory '{path}' was not found.");
        }

        return Directory.GetFiles(path, "*.cfg", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileNameWithoutExtension)
            .OfType<string>();
    }

    public static async IAsyncEnumerable<UdbCompiler> GetCompilersAsync(string udbExecutableFilePath)
    {
        var path = Path.GetDirectoryName(udbExecutableFilePath) ?? string.Empty;
        path = Path.Combine(path, UdbScriptsFolderPath);

        if (!Directory.Exists(path))
        {
            throw new DirectoryNotFoundException($"Failed to fetch UDB compilers. The directory '{path}' was not found.");
        }

        var configurationSources = Directory.GetFiles(path, "*.cfg", SearchOption.TopDirectoryOnly);

        foreach (var source in configurationSources)
        {
            string? scriptType = null;
            string? description = null;

            var lines = await File.ReadAllLinesAsync(source);
            foreach (var line in lines)
            {
                var trimmed = line.Trim();

                // Skip comments
                if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("//"))
                    continue;

                // Parse scripttype
                if (scriptType == null)
                {
                    var match = UdbConfigurationScriptTypeLineRegex().Match(trimmed);
                    if (match.Success)
                    {
                        scriptType = match.Groups[1].Value;
                    }
                }

                // Parse description
                if (description == null)
                {
                    var match = UdbConfigurationDescriptionLineRegex().Match(trimmed);
                    if (match.Success)
                    {
                        description = match.Groups[1].Value;
                    }
                }

                // Stop early if both values found
                if (scriptType != null && description != null)
                    break;
            }

            Debug.Assert(description != null);
            if (string.Equals(scriptType, "acs", StringComparison.OrdinalIgnoreCase))
            {
                yield return new UdbCompiler(description, Path.GetFileName(source));
            }
        }
    }


    internal static void StartSlade(string filePath, IEnumerable<string> paths)
    {
        var arguments = string.Join(" ", paths.Select(x => $"\"{x}\""));
        var processStartInfo = new ProcessStartInfo
        {
            FileName = filePath,
            Arguments = arguments,
            UseShellExecute = false,
        };

        // Start the process without waiting for it to finish
        Process.Start(processStartInfo);
    }
}