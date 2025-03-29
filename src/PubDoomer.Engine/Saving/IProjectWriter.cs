using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubDoomer.Engine.Saving;

/// <summary>
/// An interface that allows implementation of methods revolving around writing a project to a type of stream or output.
/// </summary>
public interface IProjectWriter
{
    IDisposable BeginBlock(string name);

    void Write(string? value);
    void Write(int? value);
    void Write(bool? value);
    void WriteEnum<T>(T? value) where T : struct, Enum;
    void WritePath(string? value);
    void WriteSignature();
}
