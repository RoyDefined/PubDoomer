using System.ComponentModel;

namespace PubDoomer.Project.Profile;

/// <summary>
/// The type of behaviour to be given to a task in case of an error.
/// </summary>
public enum ProfileTaskErrorBehaviour
{
    [Description("Never stop on errors.")] DontStop,
    [Description("Stop when an error is encountered.")] StopOnError
}