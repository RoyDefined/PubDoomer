using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubDoomer.Engine.Orchestration;

public sealed class PublishingContext
{
    public readonly string? AccCompilerExecutableFilePath;
    public readonly string? BccCompilerExecutableFilePath;
    public readonly string? GdccAccCompilerExecutableFilePath;
    public readonly string? SladeExecutableFilePath;
    public readonly string? UdbExecutableFilePath;
    public readonly string? AcsVmExecutableFilePath;

    public PublishingContext(
        string? accCompilerExecutableFilePath,
        string? bccCompilerExecutableFilePath,
        string? gdccAccCompilerExecutableFilePath,
        string? sladeExecutableFilePath,
        string? udbExecutableFilePath,
        string? acsVmExecutableFilePath)
    {
        AccCompilerExecutableFilePath = accCompilerExecutableFilePath;
        BccCompilerExecutableFilePath = bccCompilerExecutableFilePath;
        GdccAccCompilerExecutableFilePath = gdccAccCompilerExecutableFilePath;
        SladeExecutableFilePath = sladeExecutableFilePath;
        UdbExecutableFilePath = udbExecutableFilePath;
        AcsVmExecutableFilePath = acsVmExecutableFilePath;
    }
}
