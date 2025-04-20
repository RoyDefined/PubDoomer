using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubDoomer.Engine.TaskInvokation.Context;

/// <summary>
/// Represents the context which is used during invokation of tasks.
/// <br />This context is passed into tasks when they are invoked and these can be accessed for task related data
/// </summary>
public sealed class TaskInvokeContext
{
    public TaskInvokeContext()
    {
        ContextBag = [];
    }

    /// <summary>
    /// The base directory that defines where to find the project.
    /// <br /> If <see langword="null"/> no project was loaded when the context was created.
    /// </summary>
    public required string? ProjectDirectory { get; init; }

    /// <summary>
    /// The working directory that defines where tasks should invoke on.
    /// <br /> This is a mutable directory which, for example, can change by the Copy Project task which specifies a new directory to work in.
    /// <br /> If <see langword="null"/> no project was loaded when the context was created.
    /// </summary>
    public required string? WorkingDirectory { get; set; }

    /// <summary>
    /// Represents a bag containing data as a key-pair value.
    /// </summary>
    public TaskInvokeBag ContextBag { get; }
}
