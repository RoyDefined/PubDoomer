using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubDoomer.Tasks.FileSystem.Utils;

internal static class TransferUtil
{
    internal static void TransferDirectory(string sourceDir, string destDir, TransferStratergyType stratergy, bool copySubDirs = true)
    {
        var dir = new DirectoryInfo(sourceDir);
        var dirs = dir.GetDirectories();

        Directory.CreateDirectory(destDir);

        foreach (var file in dir.GetFiles())
        {
            var targetFilePath = Path.Combine(destDir, file.Name);
            TransferFile(file, targetFilePath, stratergy);
        }

        if (!copySubDirs) return;

        foreach (var subdir in dirs)
        {
            string newDestDir = Path.Combine(destDir, subdir.Name);
            TransferDirectory(subdir.FullName, newDestDir, stratergy, copySubDirs);
        }
    }

    internal static void TransferFile(FileInfo file, string destFile, TransferStratergyType stratergy)
    {
        if (stratergy == TransferStratergyType.Move)
        {
            file.MoveTo(destFile, true);
        }
        else
        {
            file.CopyTo(destFile, true);
        }
    }
}
