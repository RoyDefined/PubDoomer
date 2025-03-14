using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using PubDoomer.Project;
using PubDoomer.Utils;

// TODO: Implement caching.

namespace PubDoomer.Saving;

public sealed class SavingService(
    EncryptionService encryptionService,
    CurrentProjectProvider currentProjectProvider,
    RecentProjectCollection recentProjects,
    LocalSettings settings)
{
    private static readonly string LocalSavesFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PubDoomer");
    
    private static readonly JsonSerializerOptions ProjectSerializationOptions = new()
    {
        ReferenceHandler = ReferenceHandler.Preserve,
        WriteIndented = true,
    };

    private static readonly string RecentProjectsFile = Path.Combine(LocalSavesFolder, "recent_projects.dat");
    private static readonly string LocalSettingsFile = Path.Combine(LocalSavesFolder, "settings.dat");

    public async Task SaveProjectAsync(bool encrypt)
    {
        if (currentProjectProvider.ProjectContext == null) return;
        
        if (currentProjectProvider.ProjectContext.FilePath == null)
            throw new ArgumentException("The project does not have a valid known saving file path.");

        if (encrypt)
        {
            var encryptedBytes = await EncryptProjectAsync(currentProjectProvider.ProjectContext);
            await File.WriteAllBytesAsync(currentProjectProvider.ProjectContext.FilePath.AbsolutePath, encryptedBytes);
        }
        else
        {
            var projectJson = SerializeProject(currentProjectProvider.ProjectContext);
            await File.WriteAllTextAsync(currentProjectProvider.ProjectContext.FilePath.AbsolutePath, projectJson);
        }
    }

    public async Task LoadProjectOrDefaultAsync(string projectFilePath)
    {
        if (!File.Exists(projectFilePath)) return;

        ProjectContext? project;
        
        // If the project's extension is not json, do decrypt.
        if (Path.GetExtension(projectFilePath) != ".json")
        {
            var encryptedBytes = await File.ReadAllBytesAsync(projectFilePath);
            project = await DecryptProjectAsync(encryptedBytes);
        }
        else
        {
            var projectJson = await File.ReadAllTextAsync(projectFilePath);
            project = DeserializeProject(projectJson);
        }
        
        if (project != null) project.FilePath = new Uri(projectFilePath);

        currentProjectProvider.ProjectContext = project;
    }

    private async Task<byte[]> EncryptProjectAsync(ProjectContext project)
    {
        var json = SerializeProject(project);
        var encryptedData = await encryptionService.EncryptAsync(json);
        return encryptedData;
    }

    private async Task<ProjectContext?> DecryptProjectAsync(byte[] encryptedBytes)
    {
        var json = await encryptionService.DecryptAsync(encryptedBytes);
        var project = DeserializeProject(json);
        return project;
    }
    
    private string SerializeProject(ProjectContext project)
    {
        return JsonSerializer.Serialize(project, ProjectSerializationOptions);
    }
    
    private ProjectContext? DeserializeProject(string projectJson)
    {
        return JsonSerializer.Deserialize<ProjectContext>(projectJson, ProjectSerializationOptions);
    }

    public async Task SaveRecentProjectsAsync()
    {
        Directory.CreateDirectory(LocalSavesFolder);

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

    public async Task SaveLocalSettingsAsync()
    {
        Directory.CreateDirectory(LocalSavesFolder);

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