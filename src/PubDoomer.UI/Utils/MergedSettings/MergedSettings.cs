using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PubDoomer.Engine.Orchestration;
using PubDoomer.Project.Engine;
using PubDoomer.Project.IWad;

namespace PubDoomer.Utils.MergedSettings;

/// <summary>
/// Represents the context which contains configuration used for tasks.
/// </summary>
public class MergedSettings : PublishingContext
{
    public required string? UdbExecutableFilePath { get; init; }
    public required string? SladeExecutableFilePath { get; init; }
    public required IWadContext[] IWads { get; init; }
    public required EngineContext[] Engines { get; init; }
}
