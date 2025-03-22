using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using PubDoomer.Project;
using PubDoomer.Project.IWad;
using PubDoomer.Saving;
using PubDoomer.ViewModels.Dialogues;
using PubDoomer.Views.Dialogues;

namespace PubDoomer.Utils.MergedSettings;

/// <summary>
/// Represents a class that merges project settings and local settings together.
/// </summary>
public static class SettingsMerger
{
    /// <summary>
    /// Provides a merged context of the settings.
    /// <br/> Project settings take priority over local settings when configured.
    /// <br/> Depending on the context, settings can be <see langword="null"/>, either because the base settings were never provided or neither settings configured it.
    /// </summary>
    public static MergedSettings Merge(ProjectContext? projectContext, LocalSettings? localSettings)
    {
        static string? GetSetting(string? projectContextPath, string? localSettingsPath)
        {
            return !string.IsNullOrWhiteSpace(projectContextPath)
                ? projectContextPath.Trim()
                : localSettingsPath?.Trim();
        }

        static IEnumerable<IWadContext> MergeIWads(IEnumerable<IWadContext>? projectIWads, IEnumerable<IWadContext>? localIWads)
        {
            return (localIWads ?? [])
                .Concat(projectIWads ?? [])
                .DistinctBy(x => x.Path, StringComparer.OrdinalIgnoreCase);
        }

        return new MergedSettings
        {
            AccCompilerExecutableFilePath = GetSetting(projectContext?.AccCompilerExecutableFilePath, localSettings?.AccCompilerExecutableFilePath),
            BccCompilerExecutableFilePath = GetSetting(projectContext?.BccCompilerExecutableFilePath, localSettings?.BccCompilerExecutableFilePath),
            GdccAccCompilerExecutableFilePath = GetSetting(projectContext?.GdccCompilerExecutableFilePath, localSettings?.GdccCompilerExecutableFilePath),
            UdbExecutableFilePath = GetSetting(projectContext?.UdbExecutableFilePath, localSettings?.UdbExecutableFilePath),
            SladeExecutableFilePath = GetSetting(projectContext?.SladeExecutableFilePath, localSettings?.SladeExecutableFilePath),
            ZandronumExecutableFilePath = GetSetting(projectContext?.ZandronumExecutableFilePath, localSettings?.ZandronumExecutableFilePath),
            AcsVmExecutableFilePath = GetSetting(projectContext?.AcsVmExecutableFilePath, localSettings?.AcsVmExecutableFilePath),
            IWads = MergeIWads(projectContext?.IWads, localSettings?.IWads).ToArray(),
        };
    }
}