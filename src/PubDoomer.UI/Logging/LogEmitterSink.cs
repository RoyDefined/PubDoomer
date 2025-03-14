using System.Globalization;
using Serilog.Core;
using Serilog.Events;

namespace PubDoomer.Logging;

/// <summary>
///     Represents a sink that emits log event instances through <see cref="LogEmitter" />.
/// </summary>
public sealed class LogEmitterSink(
    LogEmitter logEmitter) : ILogEventSink
{
    /// <inheritdoc />
    public void Emit(LogEvent logEvent)
    {
        logEmitter.RaiseLogEmitted(logEvent.RenderMessage(CultureInfo.InvariantCulture));
    }
}