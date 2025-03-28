using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using PubDoomer.Encryption;
using PubDoomer.Project;
using PubDoomer.Saving;
using PubDoomer.Utils;

// TODO: Implement caching.

namespace PubDoomer.Settings.Recent;

public sealed class RecentProjectsService(
    EncryptionService encryptionService,
    RecentProjectCollection recentProjects)
{
    private static readonly string RecentProjectsFile = Path.Combine(SettingsStatics.LocalSavesFolder, "recent_projects.dat");

    public async Task SaveRecentProjectsAsync()
    {
        Directory.CreateDirectory(SettingsStatics.LocalSavesFolder);

        var json = JsonSerializer.Serialize(recentProjects.ToArray());
        var encryptedData = await encryptionService.EncryptAsync(json);

        await File.WriteAllBytesAsync(RecentProjectsFile, encryptedData);
    }

    public async Task LoadRecentProjectsAsync()
    {
        if (!File.Exists(RecentProjectsFile)) return;

        var encryptedBytes = await File.ReadAllBytesAsync(RecentProjectsFile);
        var json = await encryptionService.DecryptAsync(encryptedBytes);
        var result = JsonSerializer.Deserialize<RecentProject[]>(json) ?? [];

        foreach (var project in result) recentProjects.Add(project);
    }
}