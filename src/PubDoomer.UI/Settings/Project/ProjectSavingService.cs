using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using PubDoomer.Encryption;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using PubDoomer.Project;
using PubDoomer.Project.Archive;
using PubDoomer.Project.Engine;
using PubDoomer.Project.IWad;
using PubDoomer.Project.Maps;
using PubDoomer.Project.Profile;
using PubDoomer.Project.Tasks;
using PubDoomer.Tasks.Compile.Acc;
using PubDoomer.Tasks.Compile.Bcc;
using PubDoomer.Tasks.Compile.GdccAcc;

namespace PubDoomer.Settings.Project;

public sealed class ProjectSavingService(
    EncryptionService encryptionService,
    CurrentProjectProvider currentProjectProvider)
{
    // TODO: This has to be generic, not hardcoded in here.
    private static Dictionary<string, ProjectTaskBase> _projectTaskMap = new()
    {
        [ObservableAccCompileTask.TaskName] = new ObservableAccCompileTask(),
        [ObservableBccCompileTask.TaskName] = new ObservableBccCompileTask(),
        [ObservableGdccAccCompileTask.TaskName] = new ObservableGdccAccCompileTask(),
    };

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

        writer.Write(projectContext.Name ?? string.Empty);
        WriteConfiguration(projectContext, writer);

        // Start with tasks so that we can scaffold these on load first. This allows for references to be passed into the later profiles.
        WriteTasks(projectContext, writer);

        WriteProfiles(projectContext, writer);
        WriteIWads(projectContext, writer);
        WriteArchives(projectContext, writer);
        WriteEngines(projectContext, writer);
        WriteMaps(projectContext, writer);
    }

    public ProjectContext LoadProjectAsync(string path)
    {
        using var fileStream = File.OpenRead(path);
        var reader = new BinaryReader(fileStream);

        var projectContext = new ProjectContext();

        projectContext.Name = reader.ReadString();
        ReadConfiguration(projectContext, reader);
        ReadTasks(projectContext, reader);
        ReadProfiles(projectContext, reader);
        ReadIWads(projectContext, reader);
        ReadArchives(projectContext, reader);
        ReadEngines(projectContext, reader);
        ReadMaps(projectContext, reader);

        return projectContext;
    }

    private void WriteConfiguration(ProjectContext projectContext, BinaryWriter writer)
    {
        // TODO: This will have to be refactored to an actual dictionary. This contains dummy key data for now.
        writer.Write(6); // Number of configuration items.

        writer.Write(nameof(projectContext.AccCompilerExecutableFilePath));
        writer.Write(projectContext.AccCompilerExecutableFilePath ?? string.Empty);

        writer.Write(nameof(projectContext.BccCompilerExecutableFilePath));
        writer.Write(projectContext.BccCompilerExecutableFilePath ?? string.Empty);

        writer.Write(nameof(projectContext.GdccCompilerExecutableFilePath));
        writer.Write(projectContext.GdccCompilerExecutableFilePath ?? string.Empty);

        writer.Write(nameof(projectContext.SladeExecutableFilePath));
        writer.Write(projectContext.SladeExecutableFilePath ?? string.Empty);

        writer.Write(nameof(projectContext.UdbExecutableFilePath));
        writer.Write(projectContext.UdbExecutableFilePath ?? string.Empty);

        writer.Write(nameof(projectContext.AcsVmExecutableFilePath));
        writer.Write(projectContext.AcsVmExecutableFilePath ?? string.Empty);
    }

    private void WriteTasks(ProjectContext projectContext, BinaryWriter writer)
    {
        writer.Write(projectContext.Tasks.Count);
        foreach (var task in projectContext.Tasks)
        {
            writer.Write(task.DisplayName ?? string.Empty);
            writer.Write(task.Name ?? string.Empty);
            task.Serialize(writer);
        }
    }

    private void WriteProfiles(ProjectContext projectContext, BinaryWriter writer)
    {
        writer.Write(projectContext.Profiles.Count);
        foreach (var profile in projectContext.Profiles)
        {
            writer.Write(profile.Name ?? string.Empty);

            writer.Write(profile.Tasks.Count);
            foreach (var profileTask in profile.Tasks)
            {
                writer.Write(profileTask.Task!.Name ?? string.Empty);
                writer.Write((int)profileTask.Behaviour!.Value);
            }
        }
    }

    private void WriteIWads(ProjectContext projectContext, BinaryWriter writer)
    {
        writer.Write(projectContext.IWads.Count);
        foreach (var iwad in projectContext.IWads)
        {
            writer.Write(iwad.Name ?? string.Empty);
            writer.Write(iwad.Path ?? string.Empty);
        }
    }

    private void WriteArchives(ProjectContext projectContext, BinaryWriter writer)
    {
        writer.Write(projectContext.Archives.Count);
        foreach (var archive in projectContext.Archives)
        {
            writer.Write(archive.Name ?? string.Empty);
            writer.Write(archive.Path ?? string.Empty);
            writer.Write(archive.ExcludeFromTesting);
        }
    }

    private void WriteEngines(ProjectContext projectContext, BinaryWriter writer)
    {
        writer.Write(projectContext.Engines.Count);
        foreach (var engine in projectContext.Engines)
        {
            writer.Write(engine.Name ?? string.Empty);
            writer.Write(engine.Path ?? string.Empty);
            writer.Write((int)engine.Type);
        }
    }

    private void WriteMaps(ProjectContext projectContext, BinaryWriter writer)
    {
        writer.Write(projectContext.Maps.Count);
        foreach (var map in projectContext.Maps)
        {
            writer.Write(map.Name ?? string.Empty);
            writer.Write(map.MapLumpName ?? string.Empty);
            writer.Write(map.Path ?? string.Empty);
        }
    }

    private void ReadConfiguration(ProjectContext projectContext, BinaryReader reader)
    {
        // TODO: This will have to be refactored to an actual dictionary. This contains dummy key data for now.
        _ = reader.ReadInt32();

        _ = reader.ReadString();
        projectContext.AccCompilerExecutableFilePath = reader.ReadString();

        _ = reader.ReadString();
        projectContext.BccCompilerExecutableFilePath = reader.ReadString();

        _ = reader.ReadString();
        projectContext.GdccCompilerExecutableFilePath = reader.ReadString();

        _ = reader.ReadString();
        projectContext.SladeExecutableFilePath = reader.ReadString();

        _ = reader.ReadString();
        projectContext.UdbExecutableFilePath = reader.ReadString();

        _ = reader.ReadString();
        projectContext.AcsVmExecutableFilePath = reader.ReadString();
    }

    private void ReadTasks(ProjectContext projectContext, BinaryReader reader)
    {
        var tasksIterator = Enumerable.Range(0, reader.ReadInt32())
            .Select(x =>
            {

                var displayName = reader.ReadString();
                var task = _projectTaskMap[displayName].DeepClone();
                task.Name = reader.ReadString();
                task.Deserialize(reader);
                return task;
            });

        projectContext.Tasks = [.. tasksIterator];
    }

    private void ReadProfiles(ProjectContext projectContext, BinaryReader reader)
    {
        foreach (var _ in Enumerable.Range(0, reader.ReadInt32()))
        {
            var profile = new ProfileContext();
            profile.Name = reader.ReadString();

            var taskIterator = Enumerable.Range(0, reader.ReadInt32())
                .Select(x =>
                {
                    var task = new ProfileTask();

                    // Determine task reference from existing tasks so we retan proper references.
                    // TODO: Better support this part in the event a task can't be found.
                    var name = reader.ReadString();
                    task.Task = projectContext.Tasks.Single(x => x.Name == name);
                    task.Behaviour = (ProfileTaskErrorBehaviour)reader.ReadInt32();

                    return task;
                });

            profile.Tasks = [.. taskIterator.ToArray()];
        }
    }

    private void ReadIWads(ProjectContext projectContext, BinaryReader reader)
    {
        var iwadsIterator = Enumerable.Range(0, reader.ReadInt32())
            .Select(x => new IWadContext()
            {
                Name = reader.ReadString(),
                Path = reader.ReadString(),
            });

        projectContext.IWads = [.. iwadsIterator];
    }

    private void ReadArchives(ProjectContext projectContext, BinaryReader reader)
    {
        var archiveIterator = Enumerable.Range(0, reader.ReadInt32())
            .Select(x => new ArchiveContext()
            {
                Name = reader.ReadString(),
                Path = reader.ReadString(),
                ExcludeFromTesting = reader.ReadBoolean(),
            });

        projectContext.Archives = [.. archiveIterator];
    }

    private void ReadEngines(ProjectContext projectContext, BinaryReader reader)
    {
        var engineIterator = Enumerable.Range(0, reader.ReadInt32())
            .Select(x => new EngineContext()
            {
                Name = reader.ReadString(),
                Path = reader.ReadString(),
                Type = (EngineType)reader.ReadInt32(),
            });

        projectContext.Engines = [.. engineIterator];
    }

    private void ReadMaps(ProjectContext projectContext, BinaryReader reader)
    {
        var mapIterator = Enumerable.Range(0, reader.ReadInt32())
            .Select(x => new MapContext()
            {
                Name = reader.ReadString(),
                MapLumpName = reader.ReadString(),
                Path = reader.ReadString(),
            });

        projectContext.Maps = [.. mapIterator];
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