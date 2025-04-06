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
    internal static async Task<bool> RunProcessAsync(
        string path,
        IEnumerable<string> arguments,
        Stream stdOutStream,
        Stream stdErrStream,
        Action<string> onStdOutLine,
        Action<string> onStdErrLine)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = path,
            Arguments = string.Join(" ", arguments),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = processStartInfo };
        process.Start();

        // Additional task to output stdout and stderr.
        var stdoutReaderTask = Task.Run(async () =>
        {
            using var reader = process.StandardOutput;
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (line != null) onStdOutLine(line);
            }
        });

        var stderrReaderTask = Task.Run(async () =>
        {
            using var reader = process.StandardError;
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (line != null) onStdErrLine(line);
            }
        });

        await Task.WhenAll(stdoutReaderTask, stderrReaderTask, process.WaitForExitAsync());
        await process.StandardOutput.BaseStream.CopyToAsync(stdOutStream);
        await process.StandardError.BaseStream.CopyToAsync(stdErrStream);

        return process.ExitCode == 0;
    }

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
