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

namespace PubDoomer.Settings.Project;

public sealed class ProjectSavingService(
    EncryptionService encryptionService,
    CurrentProjectProvider currentProjectProvider)
{
    private static readonly JsonSerializerOptions ProjectSerializationOptions = new()
    {
        ReferenceHandler = ReferenceHandler.Preserve,
        WriteIndented = true,
    };

    // TODO: Deprecated
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

    // TODO: Deprecated
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

    public void SaveProject(ProjectContext projectContext, string path)
    {
        using var fileStream = File.OpenWrite(path);
        var writer = new BinaryWriter(fileStream);

        WriteProjectToStream(projectContext, writer);
    }

    public ProjectContext LoadProjectAsync(string path)
    {
        using var fileStream = File.OpenRead(path);
        var reader = new BinaryReader(fileStream);

        var projectContext = new ProjectContext();
        ReadStreamIntoProject(projectContext, reader);

        return projectContext;
    }

    private void WriteProjectToStream(ProjectContext projectContext, BinaryWriter writer)
    {
        writer.Write(projectContext.Name ?? string.Empty);
    }

    private void ReadStreamIntoProject(ProjectContext projectContext, BinaryReader reader)
    {
        projectContext.Name = reader.ReadString();
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
}