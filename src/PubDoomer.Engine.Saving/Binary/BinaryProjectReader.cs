using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace PubDoomer.Engine.Saving.Binary;

public sealed class BinaryProjectReader(
    string projectPath, Stream stream) : IProjectReader, IDisposable
{
    private readonly BinaryReader _reader = new(stream);
    public IDisposable BeginBlock() => new NoOpDisposable();
    public string ReadString() => _reader.ReadString();
    public int ReadInt32() => _reader.ReadInt32();
    public bool ReadBoolean() => _reader.ReadBoolean();
    public T ReadEnum<T>() where T : struct, Enum => (T)Enum.ToObject(typeof(T), _reader.ReadInt32());
    public string? ReadPath()
    {
        var path = _reader.ReadString();
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        return Path.GetFullPath(path, projectPath);
    }

    public ProjectSaveVersion ReadVersion()
    {
        var major = ReadInt32();
        var minor = ReadInt32();
        return new(major, minor);
    }

    public void ValidateSignature()
    {
        var @string = Encoding.UTF8.GetString(
            _reader.ReadBytes(SavingStatics.BinaryFileSignature.Length));

        if (@string != SavingStatics.BinaryFileSignature)
        {
            throw new ArgumentException("Signature mismatch. The provided stream might not be a project.");
        }
    }

    public void Dispose() => _reader.Dispose();
}
