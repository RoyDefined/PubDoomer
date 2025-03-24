using System;

namespace PubDoomer.Project.Engine;

/// <summary>
/// A provider that returns the <see cref="EngineTypeCollectionProvider"/> enum as a collection.
/// </summary>
public static class EngineTypeCollectionProvider
{
    private static EngineType[]? _cachedResult;

    public static EngineType[] Result =>
        _cachedResult ??= Enum.GetValues<EngineType>();
}