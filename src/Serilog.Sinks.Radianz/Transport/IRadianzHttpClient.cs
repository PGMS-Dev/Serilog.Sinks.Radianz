using Serilog.Sinks.Radianz.Models;

namespace Serilog.Sinks.Radianz.Transport;

/// <summary>
/// Interface for HTTP client that sends logs to Radianz backend
/// </summary>
public interface IRadianzHttpClient : IDisposable
{
    /// <summary>
    /// Sends a single log entry to the Radianz backend
    /// </summary>
    /// <param name="logEntry">The log entry to send</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task SendLogEntryAsync(RadianzLogEntry logEntry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a batch of log entries to the Radianz backend
    /// </summary>
    /// <param name="logBatch">The batch of log entries to send</param>
    /// <returns>Task representing the async operation</returns>
    Task SendLogBatchAsync(RadianzLogBatch logBatch, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends multiple log entries as a batch to the Radianz backend
    /// </summary>
    /// <param name="logEntries">The log entries to send</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task SendLogEntriesAsync(IEnumerable<RadianzLogEntry> logEntries, CancellationToken cancellationToken = default);
}

