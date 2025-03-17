using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PubDoomer.Engine.Orchestration;

namespace PubDoomer.Utils.MergedSettings;

/// <summary>
/// Represents the context which contains configuration used for tasks.
/// </summary>
public class MergedSettings : PublishingContext
{
    public required string? UdbExecutableFilePath { get; init; }
    public required string? SladeExecutableFilePath { get; init; }
}
