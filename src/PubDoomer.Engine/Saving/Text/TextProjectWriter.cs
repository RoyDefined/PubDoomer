using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubDoomer.Engine.Saving.Binary;

public sealed class TextProjectWriter(
    string projectPath, Stream stream) : IProjectWriter, IDisposable
{
    private readonly StreamWriter _writer = new(stream);
    private int _indentLevel = 0;
    private const string IndentString = "    ";

    public IDisposable BeginBlock()
    {
        _indentLevel++;
        return new IndentBlock(this);
    }

    public void Write(string? value) => WriteIndented(value ?? string.Empty);
    public void Write(int? value) => WriteIndented(value?.ToString() ?? "0");
    public void Write(bool? value) => WriteIndented(value?.ToString() ?? "false");
    public void WriteEnum<T>(T? value) where T : struct, Enum => WriteIndented(value?.ToString() ?? string.Empty);
    public void WritePath(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            WriteIndented(string.Empty);
            return;
        }
        var path = Path.GetRelativePath(projectPath, value);
        WriteIndented(path);
    }
    public void WriteSignature() => WriteIndented(SavingStatics.TextFileSignature);
    public void Dispose() => _writer.Dispose();

    private void WriteIndented(string value)
    {
        _writer.WriteLine(new string(' ', _indentLevel * IndentString.Length) + value);
    }

    private sealed class IndentBlock(TextProjectWriter writer) : IDisposable
    {
        public void Dispose() => writer._indentLevel--;
    }
}
