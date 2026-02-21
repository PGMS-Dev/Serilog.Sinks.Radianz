using Serilog.Sinks.Radianz.Configuration;
using Serilog.Sinks.Radianz.Formatting;
using Serilog.Sinks.Radianz.Transport;

namespace Serilog.Sinks.Radianz;

/// <summary>
/// Factory class for creating RadianzSink instances
/// </summary>
public static class RadianzSinkFactory
{
    /// <summary>
    /// Creates a new RadianzSink with HTTP transport
    /// </summary>
    /// <param name="options">Sink configuration options</param>
    /// <param name="httpClientFactory">Optional HTTP client factory</param>
    /// <param name="formatter">Optional custom formatter</param>
    /// <returns>A new RadianzSink instance</returns>
    public static RadianzSink CreateHttpSink(
        RadianzSinkOptions options,
        Func<HttpClient>? httpClientFactory = null,
        IRadianzLogEventFormatter? formatter = null)
    {
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        if (options.TransportType != RadianzTransportType.Http)
            throw new ArgumentException("Options must be configured for HTTP transport", nameof(options));

        options.Validate();

        formatter ??= new DefaultRadianzLogEventFormatter(options.Formatting);

        var httpClient = httpClientFactory?.Invoke() ?? new HttpClient();
        var radianzHttpClient = new RadianzHttpClient(httpClient, options.HttpOptions!, options.Retry);

        return new RadianzSink(options, formatter, radianzHttpClient);
    }

    /// <summary>
    /// Creates a new RadianzSink with message queue transport
    /// </summary>
    /// <param name="options">Sink configuration options</param>
    /// <param name="formatter">Optional custom formatter</param>
    /// <returns>A new RadianzSink instance</returns>
    public static RadianzSink CreateMessageQueueSink(
        RadianzSinkOptions options,
        IRadianzLogEventFormatter? formatter = null)
    {
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        if (options.TransportType != RadianzTransportType.MessageQueue)
            throw new ArgumentException("Options must be configured for MessageQueue transport", nameof(options));

        options.Validate();

        formatter ??= new DefaultRadianzLogEventFormatter(options.Formatting);
        var messageQueueClient = new RadianzMessageQueueClient(options.MessageQueueOptions!);

        return new RadianzSink(options, formatter, messageQueueClient);
    }
}

