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
using PubDoomer.Settings.Recent;
using PubDoomer.Utils;

namespace PubDoomer.Settings.Local;

public sealed class LocalSettingsService(
    EncryptionService encryptionService,
    LocalSettings settings)
{
    private static readonly string LocalSettingsFile = Path.Combine(SettingsStatics.LocalSavesFolder, "settings.dat");

    public async Task SaveLocalSettingsAsync()
    {
        Directory.CreateDirectory(SettingsStatics.LocalSavesFolder);

        var json = JsonSerializer.Serialize(settings);
        var encryptedData = await encryptionService.EncryptAsync(json);

        await File.WriteAllBytesAsync(LocalSettingsFile, encryptedData);
    }

    public async Task LoadLocalSettingsAsyncAsync()
    {
        if (!File.Exists(LocalSettingsFile)) return;

        var encryptedBytes = await File.ReadAllBytesAsync(LocalSettingsFile);
        var json = await encryptionService.DecryptAsync(encryptedBytes);
        JsonSerializerExtensions.Populate(settings, json);
    }
}