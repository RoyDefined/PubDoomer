using System;

namespace PubDoomer.Logging;

/// <summary>
///     Represents event args of <see cref="LogEmitter" />.
/// </summary>
/// <param name="logMessage">The log message that was emitted.</param>
public sealed class LogEmittedEventArgs(
    string logMessage) : EventArgs
{
    /// <summary>
    ///     The log message that was emitted.
    /// </summary>
    public string LogMessage { get; } = logMessage;
}