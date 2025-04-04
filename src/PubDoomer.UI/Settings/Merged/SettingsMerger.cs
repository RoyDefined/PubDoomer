using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using PubDoomer.Project;
using PubDoomer.Project.Engine;
using PubDoomer.Project.IWad;
using PubDoomer.Saving;
using PubDoomer.Tasks.AcsVM.Utils;
using PubDoomer.Tasks.Compile.Utils;
using PubDoomer.Utils;
using PubDoomer.ViewModels.Dialogues;
using PubDoomer.Views.Dialogues;

namespace PubDoomer.Settings.Merged;

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
        string? GetSetting(string key)
        {
            return (projectContext?.Configurations.ContainsKey(key),
                    localSettings?.Configurations.ContainsKey(key)) switch
            {
                (true, _) => projectContext.Configurations[key].Trim(),
                (_, true) => localSettings.Configurations[key].Trim(),
                (_, _) => null,
            };
        }

        static IEnumerable<IWadContext> MergeIWads(IEnumerable<IWadContext>? projectIwads, IEnumerable<IWadContext>? localIwads)
        {
            return (localIwads ?? [])
                .Concat(projectIwads ?? [])
                .DistinctBy(x => x.Path, StringComparer.OrdinalIgnoreCase);
        }

        static IEnumerable<EngineContext> MergeEngines(IEnumerable<EngineContext>? projectEngines, IEnumerable<EngineContext>? localEngines)
        {
            return (localEngines ?? [])
                .Concat(projectEngines ?? [])
                .DistinctBy(x => x.Path, StringComparer.OrdinalIgnoreCase);
        }

        return new MergedSettings
        {
            AccCompilerExecutableFilePath = GetSetting(CompileTaskStatics.AccCompilerExecutableFilePathKey),
            BccCompilerExecutableFilePath = GetSetting(CompileTaskStatics.BccCompilerExecutableFilePathKey),
            GdccAccCompilerExecutableFilePath = GetSetting(CompileTaskStatics.GdccAccCompilerExecutableFilePathKey),
            GdccCcCompilerExecutableFilePath = GetSetting(CompileTaskStatics.GdccCcCompilerExecutableFilePathKey),
            GdccMakeLibCompilerExecutableFilePath = GetSetting(CompileTaskStatics.GdccMakeLibCompilerExecutableFilePathKey),
            GdccLdCompilerExecutableFilePath = GetSetting(CompileTaskStatics.GdccLdCompilerExecutableFilePathKey),
            UdbExecutableFilePath = GetSetting(SavingStatics.UdbExecutableFilePathKey),
            SladeExecutableFilePath = GetSetting(SavingStatics.SladeExecutableFilePathKey),
            AcsVmExecutableFilePath = GetSetting(AcsVmTaskStatics.AcsVmExecutableFilePathKey),
            IWads = MergeIWads(projectContext?.IWads, localSettings?.IWads).ToArray(),
            Engines = MergeEngines(projectContext?.Engines, localSettings?.Engines).ToArray(),
        };
    }
}