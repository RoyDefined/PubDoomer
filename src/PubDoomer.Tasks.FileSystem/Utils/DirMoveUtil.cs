using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubDoomer.Tasks.FileSystem.Utils;

internal static class DirMoveUtil
{
    internal static void CopyDirectory(string sourceDir, string destDir, bool copySubDirs = true)
    {
        var dir = new DirectoryInfo(sourceDir);
        var dirs = dir.GetDirectories();

        Directory.CreateDirectory(destDir);

        foreach (var file in dir.GetFiles())
        {
            var targetFilePath = Path.Combine(destDir, file.Name);
            file.CopyTo(targetFilePath, true);
        }

        if (!copySubDirs) return;

        foreach (var subdir in dirs)
        {
            string newDestDir = Path.Combine(destDir, subdir.Name);
            CopyDirectory(subdir.FullName, newDestDir, copySubDirs);
        }
    }
}
