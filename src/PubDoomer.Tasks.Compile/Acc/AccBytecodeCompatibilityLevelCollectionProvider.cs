using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using System;

namespace PubDoomer.Tasks.Compile.Acc;

public static class AccBytecodeCompatibilityLevelCollectionProvider
{
    private static AccBytecodeCompatibilityLevel[]? _cachedResult;

    public static AccBytecodeCompatibilityLevel[] Result =>
        _cachedResult ??= Enum.GetValues<AccBytecodeCompatibilityLevel>();
}