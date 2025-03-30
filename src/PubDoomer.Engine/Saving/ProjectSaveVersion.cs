using System;

namespace PubDoomer.Engine.Saving;

public readonly struct ProjectSaveVersion : ICloneable, IEquatable<ProjectSaveVersion>, IComparable<ProjectSaveVersion>
{
    public ProjectSaveVersion(int major, int minor = 0)
    {
        if (major < 0 || minor < 0)
            throw new ArgumentOutOfRangeException(nameof(major), "Version numbers must be non-negative.");

        Major = major;
        Minor = minor;
    }

    public ProjectSaveVersion(ProjectSaveVersion version)
        : this(version.Major, version.Minor)
    {
    }

    public int Major { get; }
    public int Minor { get; }

    public static bool TryParse(string versionString, out ProjectSaveVersion version)
    {
        version = default;

        if (string.IsNullOrWhiteSpace(versionString) || !versionString.StartsWith('v'))
            return false;

        var parts = versionString.Substring(1).Split('.');
        if (parts.Length != 2 || !int.TryParse(parts[0], out var major) || !int.TryParse(parts[1], out var minor))
            return false;

        if(major < 0 || minor < 0)
        {
            return false;
        }

        version = new(major, minor);
        return true;
    }

    public override string ToString() => $"v{Major}.{Minor}";

    public object Clone() => new ProjectSaveVersion(this);

    public ProjectSaveVersion CloneStrict() => new ProjectSaveVersion(this);

    public bool Equals(ProjectSaveVersion other) => Major == other.Major && Minor == other.Minor;

    public override bool Equals(object? obj) => obj is ProjectSaveVersion version && Equals(version);

    public override int GetHashCode() => HashCode.Combine(Major, Minor);

    public int CompareTo(ProjectSaveVersion other)
    {
        if (Major != other.Major) return Major.CompareTo(other.Major);
        return Minor.CompareTo(other.Minor);
    }

    public static bool operator ==(ProjectSaveVersion left, ProjectSaveVersion right) => left.Equals(right);
    public static bool operator !=(ProjectSaveVersion left, ProjectSaveVersion right) => !left.Equals(right);
    public static bool operator >(ProjectSaveVersion left, ProjectSaveVersion right) => left.CompareTo(right) > 0;
    public static bool operator <(ProjectSaveVersion left, ProjectSaveVersion right) => left.CompareTo(right) < 0;
    public static bool operator >=(ProjectSaveVersion left, ProjectSaveVersion right) => left.CompareTo(right) >= 0;
    public static bool operator <=(ProjectSaveVersion left, ProjectSaveVersion right) => left.CompareTo(right) <= 0;
}
