using System.ComponentModel;

namespace PubDoomer.Tasks.Compile.Acc;

public enum AccBytecodeCompatibilityLevel
{
    [Description("None")]
    None,
    
    [Description("Use of new features is only a warning")]
    Hexen,
    
    [Description("Compatible with Hexen and old ZDooms")]
    HexenStrict,
}