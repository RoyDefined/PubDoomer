using Microsoft.Extensions.Logging;
using PubDoomer.Engine.Abstract;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubDoomer.Tasks.Compile.Utils;

internal static class TaskHelper
{
    internal static async Task WriteToFileAsync(Stream stream, string taskName, string fileName)
    {
        var directory = Path.Combine(EngineStatics.TemporaryDirectory, taskName);
        _ = Directory.CreateDirectory(directory);

        var filePath = Path.Combine(directory, fileName);
        _ = stream.Seek(0, SeekOrigin.Begin);

        using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        await stream.CopyToAsync(fileStream);
    }
}
