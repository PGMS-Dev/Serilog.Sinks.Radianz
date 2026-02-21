using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using Serilog.Sinks.Radianz.Configuration;
using Serilog.Sinks.Radianz.Models;
using System.Text;

namespace Serilog.Sinks.Radianz.Transport;

/// <summary>
/// Message queue client implementation for sending logs to Radianz backend via Azure Service Bus
/// </summary>
public class RadianzMessageQueueClient : IRadianzMessageQueueClient
{
    private readonly ServiceBusClient _serviceBusClient;
    private readonly ServiceBusSender _sender;
    private readonly MessageQueueTransportOptions _options;
    private readonly JsonSerializerSettings _jsonSettings;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the RadianzMessageQueueClient
    /// </summary>
    /// <param name="options">Message queue transport options</param>
    public RadianzMessageQueueClient(MessageQueueTransportOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _options.Validate();

        _serviceBusClient = new ServiceBusClient(_options.ConnectionString);
        _sender = _serviceBusClient.CreateSender(_options.QueueName);
        _jsonSettings = CreateJsonSettings();
    }

    /// <summary>
    /// Sends a single log entry to the message queue
    /// </summary>
    public async Task SendLogEntryAsync(RadianzLogEntry logEntry, CancellationToken cancellationToken = default)
    {
        if (logEntry == null)
            throw new ArgumentNullException(nameof(logEntry));

        var json = JsonConvert.SerializeObject(logEntry, _jsonSettings);
        var message = CreateServiceBusMessage(json, "LogEntry");

        await _sender.SendMessageAsync(message, cancellationToken);
    }

    /// <summary>
    /// Sends a batch of log entries to the message queue
    /// </summary>
    public async Task SendLogBatchAsync(RadianzLogBatch logBatch, CancellationToken cancellationToken = default)
    {
        if (logBatch == null)
            throw new ArgumentNullException(nameof(logBatch));

        var json = JsonConvert.SerializeObject(logBatch, _jsonSettings);
        var message = CreateServiceBusMessage(json, "LogBatch");

        await _sender.SendMessageAsync(message, cancellationToken);
    }

    /// <summary>
    /// Sends multiple log entries as a batch to the message queue
    /// </summary>
    public async Task SendLogEntriesAsync(IEnumerable<RadianzLogEntry> logEntries, CancellationToken cancellationToken = default)
    {
        if (logEntries == null)
            throw new ArgumentNullException(nameof(logEntries));

        var batch = new RadianzLogBatch
        {
            Entries = logEntries.ToList()
        };

        await SendLogBatchAsync(batch, cancellationToken);
    }

    private ServiceBusMessage CreateServiceBusMessage(string json, string messageType)
    {
        var messageBody = Encoding.UTF8.GetBytes(json);
        var message = new ServiceBusMessage(messageBody)
        {
            ContentType = "application/json",
            TimeToLive = TimeSpan.FromMinutes(_options.MessageTtlMinutes),
            MessageId = Guid.NewGuid().ToString()
        };

        // Add application properties for message routing and filtering
        message.ApplicationProperties["MessageType"] = messageType;
        message.ApplicationProperties["Source"] = "Serilog.Sinks.Radianz";
        message.ApplicationProperties["Timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        message.ApplicationProperties["MachineName"] = Environment.MachineName;

        return message;
    }

    private static JsonSerializerSettings CreateJsonSettings()
    {
        return new JsonSerializerSettings
        {
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Newtonsoft.Json.Formatting.None
        };
    }

    /// <summary>
    /// Disposes the message queue client and associated resources
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _sender?.DisposeAsync().AsTask().Wait(TimeSpan.FromSeconds(5));
            _serviceBusClient?.DisposeAsync().AsTask().Wait(TimeSpan.FromSeconds(5));
            _disposed = true;
        }
    }

    /// <summary>
    /// Asynchronously disposes the message queue client and associated resources
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            if (_sender != null)
                await _sender.DisposeAsync();

            if (_serviceBusClient != null)
                await _serviceBusClient.DisposeAsync();

            _disposed = true;
        }
    }
}

