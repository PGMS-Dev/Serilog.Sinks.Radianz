using Serilog.Events;
using Serilog.Sinks.Radianz.Configuration;
using Serilog.Sinks.Radianz.Formatting;
using Serilog.Sinks.Radianz.Transport;
using IBatchedLogEventSink = Serilog.Sinks.PeriodicBatching.IBatchedLogEventSink;

namespace Serilog.Sinks.Radianz;

/// <summary>
/// A Serilog sink that sends log events to the Radianz backend
/// </summary>
public class RadianzSink : IBatchedLogEventSink, IDisposable
{
    private readonly RadianzSinkOptions _options;
    private readonly IRadianzLogEventFormatter _formatter;
    private readonly IRadianzHttpClient? _httpClient;
    private readonly IRadianzMessageQueueClient? _messageQueueClient;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the RadianzSink with HTTP transport
    /// </summary>
    /// <param name="options">Sink configuration options</param>
    /// <param name="formatter">Log event formatter</param>
    /// <param name="httpClient">HTTP client for sending logs</param>
    public RadianzSink(
        RadianzSinkOptions options,
        IRadianzLogEventFormatter formatter,
        IRadianzHttpClient httpClient)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        _options.Validate();

        if (_options.TransportType != RadianzTransportType.Http)
            throw new ArgumentException("HTTP client provided but transport type is not HTTP", nameof(httpClient));
    }

    /// <summary>
    /// Initializes a new instance of the RadianzSink with message queue transport
    /// </summary>
    /// <param name="options">Sink configuration options</param>
    /// <param name="formatter">Log event formatter</param>
    /// <param name="messageQueueClient">Message queue client for sending logs</param>
    public RadianzSink(
        RadianzSinkOptions options,
        IRadianzLogEventFormatter formatter,
        IRadianzMessageQueueClient messageQueueClient)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
        _messageQueueClient = messageQueueClient ?? throw new ArgumentNullException(nameof(messageQueueClient));

        _options.Validate();

        if (_options.TransportType != RadianzTransportType.MessageQueue)
            throw new ArgumentException("Message queue client provided but transport type is not MessageQueue", nameof(messageQueueClient));
    }

    /// <summary>
    /// Emit a batch of log events to the Radianz backend
    /// </summary>
    /// <param name="events">The collection of log events to emit</param>
    /// <returns>Task representing the async operation</returns>
    public async Task EmitBatchAsync(IEnumerable<LogEvent> events)
    {
        if (events == null || !events.Any())
            return;

        try
        {
            var logEntries = _formatter.FormatLogEvents(events);

            switch (_options.TransportType)
            {
                case RadianzTransportType.Http:
                    await _httpClient!.SendLogEntriesAsync(logEntries);
                    break;

                case RadianzTransportType.MessageQueue:
                    await _messageQueueClient!.SendLogEntriesAsync(logEntries);
                    break;

                default:
                    throw new InvalidOperationException($"Unsupported transport type: {_options.TransportType}");
            }
        }
        catch (Exception ex)
        {
            // Log the error to the self-log but don't throw to avoid breaking the application
            Serilog.Debugging.SelfLog.WriteLine("Failed to emit log batch to Radianz: {0}", ex);
        }
    }

    /// <summary>
    /// Called when the sink is being disposed or the batch period has elapsed
    /// </summary>
    /// <returns>Task representing the async operation</returns>
    public Task OnEmptyBatchAsync()
    {
        // Nothing to do when batch is empty
        return Task.CompletedTask;
    }

    /// <summary>
    /// Disposes the sink and associated resources
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _httpClient?.Dispose();
            _messageQueueClient?.Dispose();
            _disposed = true;
        }
    }
}

