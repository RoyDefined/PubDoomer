using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubDoomer.Engine.Orchestration;

/// <summary>
/// Represents the context which contains configuration used for tasks.
/// </summary>
public abstract class PublishingContext
{
    public required string? AccCompilerExecutableFilePath { get; init; }
    public required string? BccCompilerExecutableFilePath { get; init; }
    public required string? GdccAccCompilerExecutableFilePath { get; init; }
    public required string? AcsVmExecutableFilePath { get; init; }
}
