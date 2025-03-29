using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubDoomer.Engine.Saving.Binary;

public sealed class BinaryProjectWriter(
    string projectPath, Stream stream) : IProjectWriter, IDisposable
{
    private readonly BinaryWriter _writer = new(stream);
    public IDisposable BeginBlock(string _) => new NoOpDisposable();
    public void Write(string? value) => _writer.Write(value ?? string.Empty);
    public void Write(int? value) => _writer.Write(value ?? 0);
    public void Write(bool? value) => _writer.Write(value ?? false);
    public void WriteEnum<T>(T? value) where T : struct, Enum => _writer.Write(Convert.ToInt32(value));
    public void WritePath(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            _writer.Write(string.Empty);
            return;
        }

        var path = Path.GetRelativePath(projectPath, value);
        Write(path);
    }
    public void WriteSignature() => _writer.Write(Encoding.UTF8.GetBytes(SavingStatics.BinaryFileSignature));
    public void Dispose() => _writer.Dispose();
}
