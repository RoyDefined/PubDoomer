using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using PubDoomer.Project.Archive;
using PubDoomer.Project.Engine;
using PubDoomer.Project.IWad;
using PubDoomer.Project.Maps;
using PubDoomer.Project.Profile;
using PubDoomer.Project.Tasks;
using PubDoomer.UI.Editor.Tasks;
using PubDoomer.Engine.TaskInvokation.TaskDefinition;
using PubDoomer.Tasks.AcsVM.Utils;
using PubDoomer.Tasks.Compile.Utils;
using PubDoomer.Utils;

namespace PubDoomer.Project;

/// <summary>
/// A provider that provides the current loaded project context in the application.
/// <br /> The current project can be set to indicate a loaded project.
/// <br /> In design mode this provider provides a design-time project.
/// </summary>
public partial class CurrentProjectProvider : ObservableObject
{
    public CurrentProjectProvider()
    {
        if (!Design.IsDesignMode) return;
        
        // Add a dummy project in design mode.
        var successTask = new ObservableSuccesfulEditorTask("Successful task :)");
        var warningTask = new ObservableWarningEditorTask("Warning task :l");
        var errorTask = new ObservableErrorEditorTask("Error task :(");

        var successProfileTask = new ProfileTask
        {
            Behaviour = ProfileTaskErrorBehaviour.DontStop,
            Task = successTask
        };

        var warningProfileTask = new ProfileTask
        {
            Behaviour = ProfileTaskErrorBehaviour.StopOnError,
            Task = warningTask
        };

        var errorProfileTask = new ProfileTask
        {
            Behaviour = ProfileTaskErrorBehaviour.StopOnError,
            Task = errorTask
        };

        ProjectContext = new ProjectContext
        {
            Name = "Project Foo",
            Configurations =
            {
                [CompileTaskStatics.AccCompilerExecutableFilePathKey] = "Path/To/ACC.exe",
                [CompileTaskStatics.BccCompilerExecutableFilePathKey] = "Path/To/BCC.exe",
                [CompileTaskStatics.GdccAccCompilerExecutableFilePathKey] = "Path/To/GDCC-ACC.exe",
                [SavingStatics.SladeExecutableFilePathKey] = "Path/To/Slade.exe",
                [SavingStatics.UdbExecutableFilePathKey] = "Path/To/UtimateDoombuilder.exe",
                [AcsVmTaskStatics.AcsVmExecutableFilePathKey] = "Path/To/ACS-VM.exe",
            },
            Engines = 
            [
                new EngineContext()
                {
                    Name = "Zandronum (latest)",
                    Path = "Path/To/Zandronum.exe",
                    Type = EngineType.Zandronum,
                },
                new EngineContext()
                {
                    Name = "GZDoom (latest)",
                    Path = "Path/To/GZDoom.exe",
                    Type = EngineType.GzDoom,
                },
                new EngineContext()
                {
                    Name = "ZDoom (latest)",
                    Path = "Path/To/ZDoom.exe",
                    Type = EngineType.Zdoom,
                },
                new EngineContext()
                {
                    Name = "Pending engine",
                    Path = "Path/To/Something.exe",
                    Type = EngineType.Unknown,
                }
            ],
            Archives =
            [
                new ArchiveContext()
                {
                    Name = "Doom project Foo",
                    Path = "Path/To/ProjectFoo",
                    ExcludeFromTesting = false,
                },
                new ArchiveContext()
                {
                    Name = "Doom testing project",
                    Path = "Path/To/TestingProject",
                    ExcludeFromTesting = true,
                }
            ],
            IWads = 
            [
                new IWadContext()
                {
                    Name = "Doom 2",
                    Path = "Path/To/Doom2.wad",
                },
                new IWadContext()
                {
                    Name = "Doom (shareware)",
                    Path = "Path/To/Doom.wad",
                }
            ],
            Tasks = [successTask, warningTask, errorTask],
            Profiles =
            [
                new ProfileContext
                {
                    Name = "Run tasks that will succeed",
                    Tasks = [successProfileTask]
                },
                new ProfileContext
                {
                    Name = "Run tasks that will warn but succeed",
                    Tasks = [successProfileTask, warningProfileTask]
                },
                new ProfileContext
                {
                    Name = "Run tasks that will warn and also error",
                    Tasks = [warningProfileTask, errorProfileTask]
                }
            ],
            Maps =
            [
                new MapContext()
                {
                    Path = "Path/To/Map01.wad",
                    Name = "Map 1",
                    MapLumpName = "Map01",
                },
                new MapContext()
                {
                    Path = "Path/To/AmazingTestMap.wad",
                    Name = "Amazing test map",
                    MapLumpName = "Amtema",
                }
            ]
        };
    }
    
    /// <summary>
    /// The project context to be provided.
    /// <br /> If the project context is <c>null</c>, no project is loaded.
    /// </summary>
    [ObservableProperty] private ProjectContext? _projectContext;
}
