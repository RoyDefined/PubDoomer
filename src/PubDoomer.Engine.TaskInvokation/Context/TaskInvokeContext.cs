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
    /// Represents a bag containing data as a key-pair value.
    /// </summary>
    public TaskInvokeBag ContextBag { get; }
}
