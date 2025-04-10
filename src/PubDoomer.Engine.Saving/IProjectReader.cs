using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubDoomer.Engine.Saving;

/// <summary>
/// An interface that allows implementation of methods revolving around reading a stream or input into a project.
/// </summary>
public interface IProjectReader
{
    IDisposable BeginBlock();

    string ReadString();
    int ReadInt32();
    bool ReadBoolean();
    T ReadEnum<T>() where T : struct, Enum;
    string? ReadPath();
    ProjectSaveVersion ReadVersion();
    void ValidateSignature();
}
