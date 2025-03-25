using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PubDoomer.Engine.Compile.Acc;

public sealed class EngineAccCompileTask : EngineCompileTaskBase
{
    private static readonly Type HandlerTypeCached = typeof(EngineAccCompileTaskHandler);

    public required bool KeepAccErrFile { get; init; }

    public override Type HandlerType => HandlerTypeCached;

    public override CompilerType Type => CompilerType.Acc;
    protected override string[] ExpectedFileExtensions { get; } = [".acs", ".txt"];
}
