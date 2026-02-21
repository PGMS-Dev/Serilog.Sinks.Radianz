using System.ComponentModel.DataAnnotations;

namespace Serilog.Sinks.Radianz.Configuration;

/// <summary>
/// Configuration options for the Radianz sink
/// </summary>
public class RadianzSinkOptions
{
    /// <summary>
    /// Transport method for sending logs to Radianz backend
    /// </summary>
    public RadianzTransportType TransportType { get; set; } = RadianzTransportType.Http;

    /// <summary>
    /// HTTP transport configuration
    /// </summary>
    public HttpTransportOptions? HttpOptions { get; set; }

    /// <summary>
    /// Message queue transport configuration
    /// </summary>
    public MessageQueueTransportOptions? MessageQueueOptions { get; set; }

    /// <summary>
    /// Formatting options for log events
    /// </summary>
    public FormattingOptions Formatting { get; set; } = new();

    /// <summary>
    /// Batching configuration for log events
    /// </summary>
    public BatchingOptions Batching { get; set; } = new();

    /// <summary>
    /// Retry policy configuration
    /// </summary>
    public RetryOptions Retry { get; set; } = new();

    /// <summary>
    /// Validates the configuration options
    /// </summary>
    public void Validate()
    {
        switch (TransportType)
        {
            case RadianzTransportType.Http:
                if (HttpOptions == null)
                    throw new InvalidOperationException("HttpOptions must be configured when using HTTP transport");
                HttpOptions.Validate();
                break;

            case RadianzTransportType.MessageQueue:
                if (MessageQueueOptions == null)
                    throw new InvalidOperationException("MessageQueueOptions must be configured when using MessageQueue transport");
                MessageQueueOptions.Validate();
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(TransportType), TransportType, "Invalid transport type");
        }
    }
}

/// <summary>
/// Transport types supported by the Radianz sink
/// </summary>
public enum RadianzTransportType
{
    /// <summary>
    /// Send logs via HTTP REST API
    /// </summary>
    Http,

    /// <summary>
    /// Send logs via message queue (Azure Service Bus)
    /// </summary>
    MessageQueue
}

/// <summary>
/// HTTP transport configuration options
/// </summary>
public class HttpTransportOptions
{
    /// <summary>
    /// Base URL of the Radianz API endpoint.
    /// Defaults to https://radianz.io (production).
    /// </summary>
    [Required]
    public string BaseUrl { get; set; } = "https://radianz.io";

    /// <summary>
    /// API endpoint path for log ingestion (relative to BaseUrl)
    /// </summary>
    public string LogEndpoint { get; set; } = "api/v1/logs";

    /// <summary>
    /// Client ID for authentication
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>
    /// API key for authentication
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// HTTP timeout in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Additional HTTP headers to include with requests
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = new();

    /// <summary>
    /// Validates the HTTP transport configuration
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(BaseUrl))
            throw new InvalidOperationException("BaseUrl is required for HTTP transport");

        if (!Uri.TryCreate(BaseUrl, UriKind.Absolute, out _))
            throw new InvalidOperationException("BaseUrl must be a valid absolute URI");

        if (TimeoutSeconds <= 0)
            throw new InvalidOperationException("TimeoutSeconds must be greater than 0");
    }
}

/// <summary>
/// Message queue transport configuration options
/// </summary>
public class MessageQueueTransportOptions
{
    /// <summary>
    /// Azure Service Bus connection string
    /// </summary>
    [Required]
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Queue name for log messages
    /// </summary>
    [Required]
    public string QueueName { get; set; } = string.Empty;

    /// <summary>
    /// Message time-to-live in minutes
    /// </summary>
    public int MessageTtlMinutes { get; set; } = 60;

    /// <summary>
    /// Validates the message queue transport configuration
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
            throw new InvalidOperationException("ConnectionString is required for MessageQueue transport");

        if (string.IsNullOrWhiteSpace(QueueName))
            throw new InvalidOperationException("QueueName is required for MessageQueue transport");

        if (MessageTtlMinutes <= 0)
            throw new InvalidOperationException("MessageTtlMinutes must be greater than 0");
    }
}

/// <summary>
/// Formatting options for log events
/// </summary>
public class FormattingOptions
{
    /// <summary>
    /// Include exception details in log events
    /// </summary>
    public bool IncludeExceptionDetails { get; set; } = true;

    /// <summary>
    /// Include scope properties in log events
    /// </summary>
    public bool IncludeScopeProperties { get; set; } = true;

    /// <summary>
    /// Maximum length for message text (0 = no limit)
    /// </summary>
    public int MaxMessageLength { get; set; } = 0;

    /// <summary>
    /// Date/time format for timestamps
    /// </summary>
    public string TimestampFormat { get; set; } = "yyyy-MM-ddTHH:mm:ss.fffZ";

    /// <summary>
    /// Additional properties to include with every log event
    /// </summary>
    public Dictionary<string, object> AdditionalProperties { get; set; } = new();
}

/// <summary>
/// Batching configuration options
/// </summary>
public class BatchingOptions
{
    /// <summary>
    /// Maximum number of events per batch
    /// </summary>
    public int BatchSizeLimit { get; set; } = 50;

    /// <summary>
    /// Maximum time to wait before sending a batch (in seconds)
    /// </summary>
    public int BatchPeriodSeconds { get; set; } = 5;

    /// <summary>
    /// Maximum number of events to queue before dropping
    /// </summary>
    public int QueueLimit { get; set; } = 10000;
}

/// <summary>
/// Retry policy configuration options
/// </summary>
public class RetryOptions
{
    /// <summary>
    /// Maximum number of retry attempts
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Base delay between retries in milliseconds
    /// </summary>
    public int BaseDelayMilliseconds { get; set; } = 1000;

    /// <summary>
    /// Maximum delay between retries in milliseconds
    /// </summary>
    public int MaxDelayMilliseconds { get; set; } = 30000;

    /// <summary>
    /// Enable exponential backoff for retries
    /// </summary>
    public bool UseExponentialBackoff { get; set; } = true;

    /// <summary>
    /// Enable jitter to avoid thundering herd
    /// </summary>
    public bool UseJitter { get; set; } = true;
}

