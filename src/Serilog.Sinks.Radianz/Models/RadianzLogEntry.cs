using Newtonsoft.Json;
using System;
using System.Reflection;

namespace Serilog.Sinks.Radianz.Models;

/// <summary>
/// Represents a log entry in the format expected by the Radianz backend
/// </summary>
public class RadianzLogEntry
{
    /// <summary>
    /// Unique identifier for the log entry
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Timestamp when the log event occurred
    /// </summary>
    [JsonProperty("timestamp")]
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Log level (Information, Warning, Error, etc.)
    /// </summary>
    [JsonProperty("level")]
    public string Level { get; set; } = string.Empty;

    /// <summary>
    /// Log message template
    /// </summary>
    [JsonProperty("messageTemplate")]
    public string MessageTemplate { get; set; } = string.Empty;

    /// <summary>
    /// Rendered log message
    /// </summary>
    [JsonProperty("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Exception details if present
    /// </summary>
    [JsonProperty("exception")]
    public RadianzExceptionInfo? Exception { get; set; }

    /// <summary>
    /// Log event properties
    /// </summary>
    [JsonProperty("properties")]
    public Dictionary<string, object> Properties { get; set; } = new();

    /// <summary>
    /// Source context (logger name)
    /// </summary>
    [JsonProperty("sourceContext")]
    public string? SourceContext { get; set; }

    /// <summary>
    /// Machine name where the log was generated
    /// </summary>
    [JsonProperty("machineName")]
    public string MachineName { get; set; } = System.Environment.MachineName;

    /// <summary>
    /// Application name or identifier
    /// </summary>
    [JsonProperty("applicationName")]
    public string? ApplicationName { get; set; }

    /// <summary>
    /// Environment (Development, Staging, Production)
    /// </summary>
    [JsonProperty("environment")]
    public string? Environment { get; set; }

    /// <summary>
    /// User ID if available
    /// </summary>
    [JsonProperty("userId")]
    public string? UserId { get; set; }

    /// <summary>
    /// Session ID if available
    /// </summary>
    [JsonProperty("sessionId")]
    public string? SessionId { get; set; }

    /// <summary>
    /// Request ID for correlation
    /// </summary>
    [JsonProperty("requestId")]
    public string? RequestId { get; set; }

    /// <summary>
    /// Additional metadata
    /// </summary>
    [JsonProperty("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Represents exception information in the Radianz log format
/// </summary>
public class RadianzExceptionInfo
{
    /// <summary>
    /// Exception type name
    /// </summary>
    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Exception message
    /// </summary>
    [JsonProperty("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Exception stack trace
    /// </summary>
    [JsonProperty("stackTrace")]
    public string? StackTrace { get; set; }

    /// <summary>
    /// Inner exception details
    /// </summary>
    [JsonProperty("innerException")]
    public RadianzExceptionInfo? InnerException { get; set; }

    /// <summary>
    /// Additional exception data
    /// </summary>
    [JsonProperty("data")]
    public Dictionary<string, object> Data { get; set; } = new();
}

/// <summary>
/// Represents a batch of log entries for bulk operations
/// </summary>
public class RadianzLogBatch
{
    /// <summary>
    /// Batch identifier
    /// </summary>
    [JsonProperty("batchId")]
    public string BatchId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Timestamp when the batch was created
    /// </summary>
    [JsonProperty("batchTimestamp")]
    public DateTime BatchTimestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Log entries in this batch
    /// </summary>
    [JsonProperty("entries")]
    public List<RadianzLogEntry> Entries { get; set; } = new();

    /// <summary>
    /// Source information for the batch
    /// </summary>
    [JsonProperty("source")]
    public RadianzBatchSource Source { get; set; } = new();
}

/// <summary>
/// Represents the source of a log batch
/// </summary>
public class RadianzBatchSource
{
    /// <summary>
    /// Application name
    /// </summary>
    [JsonProperty("applicationName")]
    public string? ApplicationName { get; set; }

    /// <summary>
    /// Machine name
    /// </summary>
    [JsonProperty("machineName")]
    public string MachineName { get; set; } = System.Environment.MachineName;

    /// <summary>
    /// Environment name
    /// </summary>
    [JsonProperty("environment")]
    public string? Environment { get; set; }

    /// <summary>
    /// Version of the application that generated the logs (auto-detected from entry assembly)
    /// </summary>
    [JsonProperty("version")]
    public string? Version { get; set; } = Assembly.GetEntryAssembly()?.GetName().Version?.ToString();
}

