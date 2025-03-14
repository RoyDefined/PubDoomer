using System;
using System.Collections.Generic;

namespace PubDoomer.Logging;

/// <summary>
///     Singleton log emitter that pushes logs emitted from <see cref="LogEmitterSink" />.
/// </summary>
public sealed class LogEmitter
{
    public Queue<string> LogMessages { get; } = new();

    /// <summary>
    ///     Called when a log message was emitted from <see cref="LogEmitterSink" />.
    /// </summary>
    public event EventHandler<LogEmittedEventArgs>? LogEmitted;

    // Raise LogEmitted witht he given log message.
    internal void RaiseLogEmitted(string logMessage)
    {
        LogMessages.Enqueue(logMessage);
        LogEmitted?.Invoke(this, new LogEmittedEventArgs(logMessage));
    }
}