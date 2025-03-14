using System.ComponentModel;

namespace PubDoomer.Project.Profile;

public enum ProfileTaskErrorBehaviour
{
    [Description("Never stop on errors.")] DontStop,

    [Description("Stop when an error is encountered.")]
    StopOnError
}