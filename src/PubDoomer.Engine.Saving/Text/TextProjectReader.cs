﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace PubDoomer.Engine.Saving.Binary;

public sealed class TextProjectReader(
    string projectPath, Stream stream) : IProjectReader, IDisposable
{
    private readonly StreamReader _reader = new(stream);
    public IDisposable BeginBlock()
    {
        _reader.ReadLine();
        return new NoOpDisposable();
    }
    public string ReadString() => _reader.ReadLine()?.Trim() ?? string.Empty;
    public int ReadInt32() => int.TryParse(_reader.ReadLine()?.Trim(), out var result) ? result : 0;
    public bool ReadBoolean() => bool.TryParse(_reader.ReadLine()?.Trim(), out var result) && result;
    public T ReadEnum<T>() where T : struct, Enum => Enum.TryParse<T>(_reader.ReadLine()?.Trim(), out var result) ? result : default;

    public ProjectSaveVersion ReadVersion()
    {
        var line = ReadString();
        if (!ProjectSaveVersion.TryParse(line, out var version))
        {
            throw new FormatException($"Line is not a valid version: {line}");
        }
        return version;
    }

    public void ValidateSignature()
    {
        var signature = _reader.ReadLine()?.Trim();
        if (signature != SavingStatics.TextFileSignature)
        {
            throw new ArgumentException("Signature mismatch. The provided stream might not be a project.");
        }
    }

    public void Dispose() => _reader.Dispose();
}
