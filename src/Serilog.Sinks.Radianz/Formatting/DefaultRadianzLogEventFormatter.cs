using Serilog.Events;
using Serilog.Sinks.Radianz.Configuration;
using Serilog.Sinks.Radianz.Models;
using System.Text;
using System.IO;
using System.Globalization;

namespace Serilog.Sinks.Radianz.Formatting;

/// <summary>
/// Default implementation of IRadianzLogEventFormatter
/// </summary>
public class DefaultRadianzLogEventFormatter : IRadianzLogEventFormatter
{
    private readonly FormattingOptions _options;

    /// <summary>
    /// Initializes a new instance of the DefaultRadianzLogEventFormatter
    /// </summary>
    /// <param name="options">Formatting options</param>
    public DefaultRadianzLogEventFormatter(FormattingOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Formats a single log event into a Radianz log entry
    /// </summary>
    /// <param name="logEvent">The Serilog log event to format</param>
    /// <returns>A formatted Radianz log entry</returns>
    public RadianzLogEntry FormatLogEvent(LogEvent logEvent)
    {
        if (logEvent == null)
            throw new ArgumentNullException(nameof(logEvent));

        var entry = new RadianzLogEntry
        {
            Timestamp = logEvent.Timestamp.UtcDateTime,
            Level = logEvent.Level.ToString(),
            MessageTemplate = logEvent.MessageTemplate.Text,
            Message = FormatMessage(logEvent)
        };

        // Add exception information if present
        if (logEvent.Exception != null && _options.IncludeExceptionDetails)
        {
            entry.Exception = FormatException(logEvent.Exception);
        }

        // Add properties
        if (_options.IncludeScopeProperties)
        {
            foreach (var property in logEvent.Properties)
            {
                entry.Properties[property.Key] = FormatPropertyValue(property.Value);
            }
        }

        // Extract common properties
        ExtractCommonProperties(logEvent, entry);

        // Add additional configured properties
        foreach (var additionalProperty in _options.AdditionalProperties)
        {
            entry.Metadata[additionalProperty.Key] = additionalProperty.Value;
        }

        return entry;
    }

    /// <summary>
    /// Formats multiple log events into a batch of Radianz log entries
    /// </summary>
    /// <param name="logEvents">The collection of Serilog log events to format</param>
    /// <returns>A collection of formatted Radianz log entries</returns>
    public IEnumerable<RadianzLogEntry> FormatLogEvents(IEnumerable<LogEvent> logEvents)
    {
        if (logEvents == null)
            throw new ArgumentNullException(nameof(logEvents));

        return logEvents.Select(FormatLogEvent);
    }

    private string FormatMessage(LogEvent logEvent)
    {
        using var writer = new StringWriter(CultureInfo.InvariantCulture);
        logEvent.MessageTemplate.Render(logEvent.Properties, writer, CultureInfo.InvariantCulture);
        var message = writer.ToString();

        if (_options.MaxMessageLength > 0 && message.Length > _options.MaxMessageLength)
        {
            message = message.Substring(0, _options.MaxMessageLength) + "...";
        }

        return message;
    }

    private RadianzExceptionInfo FormatException(Exception exception)
    {
        var exceptionInfo = new RadianzExceptionInfo
        {
            Type = exception.GetType().FullName ?? exception.GetType().Name,
            Message = exception.Message,
            StackTrace = exception.StackTrace
        };

        // Add exception data
        foreach (var key in exception.Data.Keys)
        {
            if (key != null)
            {
                exceptionInfo.Data[key.ToString()!] = exception.Data[key] ?? string.Empty;
            }
        }

        // Handle inner exception
        if (exception.InnerException != null)
        {
            exceptionInfo.InnerException = FormatException(exception.InnerException);
        }

        return exceptionInfo;
    }

    private object FormatPropertyValue(LogEventPropertyValue propertyValue)
    {
        return propertyValue switch
        {
            ScalarValue scalar => scalar.Value ?? string.Empty,
            SequenceValue sequence => sequence.Elements.Select(FormatPropertyValue).ToArray(),
            StructureValue structure => structure.Properties.ToDictionary(
                p => p.Name,
                p => FormatPropertyValue(p.Value)
            ),
            DictionaryValue dictionary => dictionary.Elements.ToDictionary(
                kvp => FormatPropertyValue(kvp.Key).ToString() ?? string.Empty,
                kvp => FormatPropertyValue(kvp.Value)
            ),
            _ => propertyValue.ToString()
        };
    }

    private void ExtractCommonProperties(LogEvent logEvent, RadianzLogEntry entry)
    {
        // Extract source context
        if (logEvent.Properties.TryGetValue("SourceContext", out var sourceContext))
        {
            entry.SourceContext = FormatPropertyValue(sourceContext).ToString();
        }

        // Extract application name
        if (logEvent.Properties.TryGetValue("ApplicationName", out var appName))
        {
            entry.ApplicationName = FormatPropertyValue(appName).ToString();
        }

        // Extract environment
        if (logEvent.Properties.TryGetValue("Environment", out var environment))
        {
            entry.Environment = FormatPropertyValue(environment).ToString();
        }

        // Extract user ID
        if (logEvent.Properties.TryGetValue("UserId", out var userId))
        {
            entry.UserId = FormatPropertyValue(userId).ToString();
        }

        // Extract session ID
        if (logEvent.Properties.TryGetValue("SessionId", out var sessionId))
        {
            entry.SessionId = FormatPropertyValue(sessionId).ToString();
        }

        // Extract request ID
        if (logEvent.Properties.TryGetValue("RequestId", out var requestId))
        {
            entry.RequestId = FormatPropertyValue(requestId).ToString();
        }
    }
}

