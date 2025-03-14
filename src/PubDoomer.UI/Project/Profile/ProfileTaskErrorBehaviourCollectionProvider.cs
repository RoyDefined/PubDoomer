using System;

namespace PubDoomer.Project.Profile;

public static class ProfileTaskErrorBehaviourCollectionProvider
{
    private static ProfileTaskErrorBehaviour[]? _cachedResult;

    public static ProfileTaskErrorBehaviour[] Result =>
        _cachedResult ??= Enum.GetValues<ProfileTaskErrorBehaviour>();
}