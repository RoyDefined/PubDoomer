using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PubDoomer.Engine.Orchestration;
using PubDoomer.Project.Engine;
using PubDoomer.Project.IWad;

namespace PubDoomer.Settings.Merged;

/// <summary>
/// Represents the merged settings of the project settings and local settings.
/// </summary>
public sealed class MergedSettings
{
    public required string? AccCompilerExecutableFilePath { get; init; }
    public required string? BccCompilerExecutableFilePath { get; init; }
    public required string? GdccAccCompilerExecutableFilePath { get; init; }
    public required string? AcsVmExecutableFilePath { get; init; }
    public required string? UdbExecutableFilePath { get; init; }
    public required string? SladeExecutableFilePath { get; init; }
    public required IWadContext[] IWads { get; init; }
    public required EngineContext[] Engines { get; init; }
}
