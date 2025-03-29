﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using PubDoomer.Encryption;
using PubDoomer.Engine.Saving;
using PubDoomer.Engine.Saving.Binary;
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

    public void SaveProject(ProjectContext projectContext, string path, ProjectReadingWritingType writerType)
    {
        using var fileStream = File.OpenWrite(path);
        IProjectWriter writer = writerType switch
        {
            ProjectReadingWritingType.Binary => new BinaryProjectWriter(path, fileStream),
            _ => throw new ArgumentException($"Writer not found for type {writerType}."),
        };

        try
        {
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
        finally
        {
            if (writer is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    public ProjectContext LoadProjectAsync(string path, ProjectReadingWritingType readerType)
    {
        using var fileStream = File.OpenRead(path);

        IProjectReader reader = readerType switch
        {
            ProjectReadingWritingType.Binary => new BinaryProjectReader(path, fileStream),
            _ => throw new ArgumentException($"Reader not found for type {readerType}."),
        };

        try
        {
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
        finally
        {
            if (reader is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    private void WriteConfiguration(ProjectContext projectContext, IProjectWriter writer)
    {
        void WriteConfiguration(string name, string? value)
        {
            writer.Write(name);
            writer.WritePath(value ?? string.Empty);
        }

        // TODO: This will have to be refactored to an actual dictionary. This contains dummy key data for now.

        // Number of configuration items.
        writer.Write(6);

        WriteConfiguration(nameof(projectContext.AccCompilerExecutableFilePath), projectContext.AccCompilerExecutableFilePath);
        WriteConfiguration(nameof(projectContext.BccCompilerExecutableFilePath), projectContext.BccCompilerExecutableFilePath);
        WriteConfiguration(nameof(projectContext.GdccCompilerExecutableFilePath), projectContext.GdccCompilerExecutableFilePath);
        WriteConfiguration(nameof(projectContext.SladeExecutableFilePath), projectContext.SladeExecutableFilePath);
        WriteConfiguration(nameof(projectContext.UdbExecutableFilePath), projectContext.UdbExecutableFilePath);
        WriteConfiguration(nameof(projectContext.AcsVmExecutableFilePath), projectContext.AcsVmExecutableFilePath);
    }

    private void WriteTasks(ProjectContext projectContext, IProjectWriter writer)
    {
        writer.Write(projectContext.Tasks.Count);
        foreach (var task in projectContext.Tasks)
        {
            writer.Write(task.DisplayName ?? string.Empty);
            writer.Write(task.Name ?? string.Empty);
            task.Serialize(writer);
        }
    }

    private void WriteProfiles(ProjectContext projectContext, IProjectWriter writer)
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

    private void WriteIWads(ProjectContext projectContext, IProjectWriter writer)
    {
        writer.Write(projectContext.IWads.Count);
        foreach (var iwad in projectContext.IWads)
        {
            writer.Write(iwad.Name ?? string.Empty);
            writer.WritePath(iwad.Path ?? string.Empty);
        }
    }

    private void WriteArchives(ProjectContext projectContext, IProjectWriter writer)
    {
        writer.Write(projectContext.Archives.Count);
        foreach (var archive in projectContext.Archives)
        {
            writer.Write(archive.Name ?? string.Empty);
            writer.WritePath(archive.Path ?? string.Empty);
            writer.Write(archive.ExcludeFromTesting);
        }
    }

    private void WriteEngines(ProjectContext projectContext, IProjectWriter writer)
    {
        writer.Write(projectContext.Engines.Count);
        foreach (var engine in projectContext.Engines)
        {
            writer.Write(engine.Name ?? string.Empty);
            writer.WritePath(engine.Path ?? string.Empty);
            writer.Write((int)engine.Type);
        }
    }

    private void WriteMaps(ProjectContext projectContext, IProjectWriter writer)
    {
        writer.Write(projectContext.Maps.Count);
        foreach (var map in projectContext.Maps)
        {
            writer.Write(map.Name ?? string.Empty);
            writer.Write(map.MapLumpName ?? string.Empty);
            writer.WritePath(map.Path ?? string.Empty);
        }
    }

    private void ReadConfiguration(ProjectContext projectContext, IProjectReader reader)
    {
        string ReadConfiguration()
        {
            _ = reader.ReadString();
            return reader.ReadPath();
        }

        // TODO: This will have to be refactored to an actual dictionary. This contains dummy key data for now.
        _ = reader.ReadInt32();

        projectContext.AccCompilerExecutableFilePath = ReadConfiguration();
        projectContext.BccCompilerExecutableFilePath = ReadConfiguration();
        projectContext.GdccCompilerExecutableFilePath = ReadConfiguration();
        projectContext.SladeExecutableFilePath = ReadConfiguration();
        projectContext.UdbExecutableFilePath = ReadConfiguration();
        projectContext.AcsVmExecutableFilePath = ReadConfiguration();
    }

    private void ReadTasks(ProjectContext projectContext, IProjectReader reader)
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

    private void ReadProfiles(ProjectContext projectContext, IProjectReader reader)
    {
        var profileIterator = Enumerable.Range(0, reader.ReadInt32())
            .Select(x =>
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
                return profile;
            });

        projectContext.Profiles = [.. profileIterator];
    }

    private void ReadIWads(ProjectContext projectContext, IProjectReader reader)
    {
        var iwadsIterator = Enumerable.Range(0, reader.ReadInt32())
            .Select(x => new IWadContext()
            {
                Name = reader.ReadString(),
                Path = reader.ReadPath(),
            });

        projectContext.IWads = [.. iwadsIterator];
    }

    private void ReadArchives(ProjectContext projectContext, IProjectReader reader)
    {
        var archiveIterator = Enumerable.Range(0, reader.ReadInt32())
            .Select(x => new ArchiveContext()
            {
                Name = reader.ReadString(),
                Path = reader.ReadPath(),
                ExcludeFromTesting = reader.ReadBoolean(),
            });

        projectContext.Archives = [.. archiveIterator];
    }

    private void ReadEngines(ProjectContext projectContext, IProjectReader reader)
    {
        var engineIterator = Enumerable.Range(0, reader.ReadInt32())
            .Select(x => new EngineContext()
            {
                Name = reader.ReadString(),
                Path = reader.ReadPath(),
                Type = (EngineType)reader.ReadInt32(),
            });

        projectContext.Engines = [.. engineIterator];
    }

    private void ReadMaps(ProjectContext projectContext, IProjectReader reader)
    {
        var mapIterator = Enumerable.Range(0, reader.ReadInt32())
            .Select(x => new MapContext()
            {
                Name = reader.ReadString(),
                MapLumpName = reader.ReadString(),
                Path = reader.ReadPath(),
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