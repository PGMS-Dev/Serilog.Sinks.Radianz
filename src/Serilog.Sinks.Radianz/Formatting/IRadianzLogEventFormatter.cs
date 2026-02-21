using Serilog.Events;
using Serilog.Sinks.Radianz.Models;

namespace Serilog.Sinks.Radianz.Formatting;

/// <summary>
/// Interface for formatting Serilog events into Radianz-compatible format
/// </summary>
public interface IRadianzLogEventFormatter
{
    /// <summary>
    /// Formats a single log event into a Radianz log entry
    /// </summary>
    /// <param name="logEvent">The Serilog log event to format</param>
    /// <returns>A formatted Radianz log entry</returns>
    RadianzLogEntry FormatLogEvent(LogEvent logEvent);

    /// <summary>
    /// Formats multiple log events into a batch of Radianz log entries
    /// </summary>
    /// <param name="logEvents">The collection of Serilog log events to format</param>
    /// <returns>A collection of formatted Radianz log entries</returns>
    IEnumerable<RadianzLogEntry> FormatLogEvents(IEnumerable<LogEvent> logEvents);
}

