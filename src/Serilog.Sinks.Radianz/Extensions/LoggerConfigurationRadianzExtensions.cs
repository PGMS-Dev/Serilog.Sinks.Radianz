using Serilog.Configuration;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;
using Serilog.Sinks.Radianz.Configuration;
using Serilog.Sinks.Radianz.Formatting;
using BatchingOptions = Serilog.Sinks.Radianz.Configuration.BatchingOptions;

namespace Serilog.Sinks.Radianz.Extensions;

/// <summary>
/// Extension methods for configuring the Radianz sink
/// </summary>
public static class LoggerConfigurationRadianzExtensions
{
    /// <summary>
    /// Adds a Radianz sink with HTTP transport to the logger configuration.
    /// BaseUrl defaults to https://radianz.io (production) if not specified.
    /// </summary>
    public static LoggerConfiguration RadianzHttp(
        this LoggerSinkConfiguration loggerSinkConfiguration,
        string? clientId = null,
        string? apiKey = null,
        string? baseUrl = null,
        LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose,
        int batchSizeLimit = 50,
        TimeSpan? batchPeriod = null,
        int queueLimit = 10000,
        IRadianzLogEventFormatter? formatter = null)
    {
        if (loggerSinkConfiguration == null)
            throw new ArgumentNullException(nameof(loggerSinkConfiguration));

        var effectiveBaseUrl = string.IsNullOrWhiteSpace(baseUrl) ? "https://radianz.io" : baseUrl;

        var options = new RadianzSinkOptions
        {
            TransportType = RadianzTransportType.Http,
            HttpOptions = new HttpTransportOptions
            {
                BaseUrl = effectiveBaseUrl,
                ClientId = clientId,
                ApiKey = apiKey
            },
            Batching = new BatchingOptions
            {
                BatchSizeLimit = batchSizeLimit,
                BatchPeriodSeconds = (int)(batchPeriod ?? TimeSpan.FromSeconds(5)).TotalSeconds,
                QueueLimit = queueLimit
            }
        };

        return loggerSinkConfiguration.RadianzWithOptions(options, restrictedToMinimumLevel, formatter);
    }

    /// <summary>
    /// Adds a Radianz sink with message queue transport to the logger configuration
    /// </summary>
    public static LoggerConfiguration RadianzMessageQueue(
        this LoggerSinkConfiguration loggerSinkConfiguration,
        string connectionString,
        string queueName,
        LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose,
        int batchSizeLimit = 50,
        TimeSpan? batchPeriod = null,
        int queueLimit = 10000,
        IRadianzLogEventFormatter? formatter = null)
    {
        if (loggerSinkConfiguration == null)
            throw new ArgumentNullException(nameof(loggerSinkConfiguration));

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));

        if (string.IsNullOrWhiteSpace(queueName))
            throw new ArgumentException("Queue name cannot be null or empty", nameof(queueName));

        var options = new RadianzSinkOptions
        {
            TransportType = RadianzTransportType.MessageQueue,
            MessageQueueOptions = new MessageQueueTransportOptions
            {
                ConnectionString = connectionString,
                QueueName = queueName
            },
            Batching = new BatchingOptions
            {
                BatchSizeLimit = batchSizeLimit,
                BatchPeriodSeconds = (int)(batchPeriod ?? TimeSpan.FromSeconds(5)).TotalSeconds,
                QueueLimit = queueLimit
            }
        };

        return loggerSinkConfiguration.RadianzWithOptions(options, restrictedToMinimumLevel, formatter);
    }

    /// <summary>
    /// Adds a Radianz sink with custom options to the logger configuration
    /// </summary>
    public static LoggerConfiguration RadianzWithOptions(
        this LoggerSinkConfiguration loggerSinkConfiguration,
        RadianzSinkOptions options,
        LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose,
        IRadianzLogEventFormatter? formatter = null)
    {
        if (loggerSinkConfiguration == null)
            throw new ArgumentNullException(nameof(loggerSinkConfiguration));

        if (options == null)
            throw new ArgumentNullException(nameof(options));

        options.Validate();

        RadianzSink sink = options.TransportType switch
        {
            RadianzTransportType.Http => RadianzSinkFactory.CreateHttpSink(options, formatter: formatter),
            RadianzTransportType.MessageQueue => RadianzSinkFactory.CreateMessageQueueSink(options, formatter),
            _ => throw new ArgumentException($"Unsupported transport type: {options.TransportType}")
        };

        var batchingOptions = new PeriodicBatchingSinkOptions
        {
            BatchSizeLimit = options.Batching.BatchSizeLimit,
            Period = TimeSpan.FromSeconds(options.Batching.BatchPeriodSeconds),
            QueueLimit = options.Batching.QueueLimit
        };

        var batchingSink = new PeriodicBatchingSink(sink, batchingOptions);

        return loggerSinkConfiguration.Sink(batchingSink, restrictedToMinimumLevel);
    }

    /// <summary>
    /// Adds a Radianz sink with configuration action to the logger configuration
    /// </summary>
    public static LoggerConfiguration Radianz(
        this LoggerSinkConfiguration loggerSinkConfiguration,
        Action<RadianzSinkOptions> configureOptions,
        LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose,
        IRadianzLogEventFormatter? formatter = null)
    {
        if (loggerSinkConfiguration == null)
            throw new ArgumentNullException(nameof(loggerSinkConfiguration));

        if (configureOptions == null)
            throw new ArgumentNullException(nameof(configureOptions));

        var options = new RadianzSinkOptions();
        configureOptions(options);

        return loggerSinkConfiguration.RadianzWithOptions(options, restrictedToMinimumLevel, formatter);
    }
}

