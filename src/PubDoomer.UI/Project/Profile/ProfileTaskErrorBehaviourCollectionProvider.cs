using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using System;

namespace PubDoomer.Project.Profile;

/// <summary>
/// A provider that returns the <see cref="ProfileTaskErrorBehaviour"/> enum as a collection.
/// </summary>
public static class ProfileTaskErrorBehaviourCollectionProvider
{
    private static ProfileTaskErrorBehaviour[]? _cachedResult;

    public static ProfileTaskErrorBehaviour[] Result =>
        _cachedResult ??= Enum.GetValues<ProfileTaskErrorBehaviour>();
}