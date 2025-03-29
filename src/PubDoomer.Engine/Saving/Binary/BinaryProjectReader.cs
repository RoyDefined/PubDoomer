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
    public string ReadString() => _reader.ReadString();
    public int ReadInt32() => _reader.ReadInt32();
    public bool ReadBoolean() => _reader.ReadBoolean();
    public T ReadEnum<T>() where T : struct, Enum => (T)Enum.ToObject(typeof(T), _reader.ReadInt32());
    public string ReadPath() => Path.GetFullPath(ReadString(), projectPath);

    public void ValidateSignature()
    {
        var @string = Encoding.UTF8.GetString(
            _reader.ReadBytes(BinarySavingStatic.BinaryFileSignature.Length));

        if (@string != BinarySavingStatic.BinaryFileSignature)
        {
            throw new ArgumentException("Signature mismatch. The provided stream might not be a project.");
        }
    }

    public void Dispose() => _reader.Dispose();
}
