using System;
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
using PubDoomer.Tasks.Compile.GdccCc;
using PubDoomer.Tasks.FileSystem;
using PubDoomer.Tasks.FileSystem.CopyFile;
using PubDoomer.Tasks.FileSystem.CopyFolder;
using PubDoomer.Tasks.FileSystem.CopyProject;
using PubDoomer.Tasks.FileSystem.DeleteFile;
using PubDoomer.Tasks.FileSystem.DeleteFolder;
using PubDoomer.Tasks.FileSystem.MoveFile;
using PubDoomer.Tasks.FileSystem.MoveFolder;
using PubDoomer.Tasks.FileSystem.ZipFolder;

namespace PubDoomer.Settings.Project;

public sealed class ProjectSavingService
{
    // TODO: This has to be generic, not hardcoded in here.
    private static Dictionary<string, ProjectTaskBase> _projectTaskMap = new()
    {
        [ObservableAccCompileTask.TaskName] = new ObservableAccCompileTask(),
        [ObservableBccCompileTask.TaskName] = new ObservableBccCompileTask(),
        [ObservableGdccAccCompileTask.TaskName] = new ObservableGdccAccCompileTask(),
        [ObservableGdccCcCompileTask.TaskName] = new ObservableGdccCcCompileTask(),
        [ObservableCopyProjectTask.TaskName] = new ObservableCopyProjectTask(),
        [ObservableCopyFolderTask.TaskName] = new ObservableCopyFolderTask(),
        [ObservableMoveFolderTask.TaskName] = new ObservableMoveFolderTask(),
        [ObservableCopyFileTask.TaskName] = new ObservableCopyFileTask(),
        [ObservableMoveFileTask.TaskName] = new ObservableMoveFileTask(),
        [ObservableZipFolderTask.TaskName] = new ObservableZipFolderTask(),
        [ObservableDeleteFolderTask.TaskName] = new ObservableDeleteFolderTask(),
        [ObservableDeleteFileTask.TaskName] = new ObservableDeleteFileTask(),
    };

    /// <summary>
    /// This is the version written to files.
    /// <br /> When reading, this version will not be used. Instead it will be conditionally handled.
    /// </summary>
    private readonly ProjectSaveVersion _latestSaveVersion = new(0, 5);

    public void SaveProject(ProjectContext projectContext, Stream stream)
    {
        // Get the writer based on the save type.
        IProjectWriter writer = projectContext.SaveType switch
        {
            ProjectSaveType.Binary => new BinaryProjectWriter(projectContext.FolderPath, stream),
            ProjectSaveType.Text => new TextProjectWriter(projectContext.FolderPath, stream),
            _ => throw new ArgumentException($"Writer not found for type {projectContext.SaveType}."),
        };

        try
        {
            writer.WriteSignature();
            writer.WriteVersion(_latestSaveVersion);
            
            writer.Write(projectContext.Name);
            WriteConfiguration(projectContext, writer);

            // Start with tasks so that we can scaffold these on load first. This allows for references to be passed into the later profiles.
            using (writer.BeginBlock("Tasks")) WriteTasks(projectContext, writer);

            using (writer.BeginBlock("Profiles")) WriteProfiles(projectContext, writer);
            using (writer.BeginBlock("IWads")) WriteIWads(projectContext, writer);
            using (writer.BeginBlock("Archives")) WriteArchives(projectContext, writer);
            using (writer.BeginBlock("Engines")) WriteEngines(projectContext, writer);
            using (writer.BeginBlock("Maps")) WriteMaps(projectContext, writer);
        }
        finally
        {
            if (writer is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    public ProjectContext LoadProject(string folderPath, Stream fileStream, ProjectSaveType saveType)
    {
        IProjectReader reader = saveType switch
        {
            ProjectSaveType.Binary => new BinaryProjectReader(folderPath, fileStream),
            ProjectSaveType.Text => new TextProjectReader(folderPath, fileStream),
            _ => throw new ArgumentException($"Reader not found for type {saveType}."),
        };

        try
        {
            var projectContext = new ProjectContext();
            reader.ValidateSignature();
            var version = reader.ReadVersion();

            projectContext.Name = reader.ReadString();
            ReadConfiguration(projectContext, reader, version);

            using (reader.BeginBlock()) ReadTasks(projectContext, reader, version);
            using (reader.BeginBlock()) ReadProfiles(projectContext, reader, version);
            using (reader.BeginBlock()) ReadIWads(projectContext, reader, version);
            using (reader.BeginBlock()) ReadArchives(projectContext, reader, version);
            using (reader.BeginBlock()) ReadEngines(projectContext, reader, version);
            using (reader.BeginBlock()) ReadMaps(projectContext, reader, version);

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
        void WriteConfiguration(string name, string value)
        {
            writer.Write(name);
            writer.Write(value);
        }

        // Number of configuration items.
        writer.Write(projectContext.Configurations.Count);

        foreach (var (key, value) in projectContext.Configurations)
        {
            WriteConfiguration(key, value);
        }
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
            writer.Write(iwad.Path ?? string.Empty);
        }
    }

    private void WriteArchives(ProjectContext projectContext, IProjectWriter writer)
    {
        writer.Write(projectContext.Archives.Count);
        foreach (var archive in projectContext.Archives)
        {
            writer.Write(archive.Name ?? string.Empty);
            writer.Write(archive.Path ?? string.Empty);
            writer.Write(archive.ExcludeFromTesting);
        }
    }

    private void WriteEngines(ProjectContext projectContext, IProjectWriter writer)
    {
        writer.Write(projectContext.Engines.Count);
        foreach (var engine in projectContext.Engines)
        {
            writer.Write(engine.Name ?? string.Empty);
            writer.Write(engine.Path ?? string.Empty);
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
            writer.Write(map.Path ?? string.Empty);
        }
    }

    private void ReadConfiguration(ProjectContext projectContext, IProjectReader reader, ProjectSaveVersion _)
    {
        (string key, string value) ReadConfiguration()
        {
            return (reader.ReadString(), reader.ReadString() ?? string.Empty);
        }

        var configurationIterator = Enumerable.Range(0, reader.ReadInt32())
            .Select(x => ReadConfiguration());

        foreach(var (key, value) in configurationIterator)
        {
            projectContext.Configurations[key] = value;
        }
    }

    private void ReadTasks(ProjectContext projectContext, IProjectReader reader, ProjectSaveVersion version)
    {
        var tasksIterator = Enumerable.Range(0, reader.ReadInt32())
            .Select(x =>
            {

                var displayName = reader.ReadString();
                var task = _projectTaskMap[displayName].DeepClone();
                task.Name = reader.ReadString();
                task.Deserialize(reader, version);
                return task;
            });

        projectContext.Tasks = [.. tasksIterator];
    }

    private void ReadProfiles(ProjectContext projectContext, IProjectReader reader, ProjectSaveVersion _)
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

                        // Determine task reference from existing tasks so we retain proper references.
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

    private void ReadIWads(ProjectContext projectContext, IProjectReader reader, ProjectSaveVersion _)
    {
        var iwadsIterator = Enumerable.Range(0, reader.ReadInt32())
            .Select(x => new IWadContext()
            {
                Name = reader.ReadString(),
                Path = reader.ReadString(),
            });

        projectContext.IWads = [.. iwadsIterator];
    }

    private void ReadArchives(ProjectContext projectContext, IProjectReader reader, ProjectSaveVersion _)
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

    private void ReadEngines(ProjectContext projectContext, IProjectReader reader, ProjectSaveVersion _)
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

    private void ReadMaps(ProjectContext projectContext, IProjectReader reader, ProjectSaveVersion _)
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
}