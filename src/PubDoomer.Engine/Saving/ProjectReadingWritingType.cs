using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubDoomer.Engine.Saving;

/// <summary>
/// Specifies the type of reader/writer to use for reading a stream or input into a project or write a project into a stream or input.
/// </summary>
public enum ProjectReadingWritingType
{
    None,
    Binary,
}
