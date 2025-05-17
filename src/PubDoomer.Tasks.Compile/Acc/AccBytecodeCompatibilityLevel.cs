using System.ComponentModel;

namespace PubDoomer.Tasks.Compile.Acc;

public enum AccBytecodeCompatibilityLevel
{
    [Description("No compatibility")]
    None,
    
    [Description("Use of ZDoom features is only a warning")]
    Hexen,
    
    [Description("Compatible with Hexen and old ZDooms")]
    HexenStrict,
}