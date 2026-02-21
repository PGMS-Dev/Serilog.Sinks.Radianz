using Newtonsoft.Json;
using Polly;
using Polly.Extensions.Http;
using Serilog.Sinks.Radianz.Configuration;
using Serilog.Sinks.Radianz.Models;
using System.Net;
using System.Reflection;
using System.Text;

namespace Serilog.Sinks.Radianz.Transport;

/// <summary>
/// HTTP client implementation for sending logs to Radianz backend
/// </summary>
public class RadianzHttpClient : IRadianzHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly HttpTransportOptions _options;
    private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;
    private readonly JsonSerializerSettings _jsonSettings;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the RadianzHttpClient
    /// </summary>
    /// <param name="httpClient">HTTP client instance</param>
    /// <param name="options">HTTP transport options</param>
    /// <param name="retryOptions">Retry policy options</param>
    public RadianzHttpClient(HttpClient httpClient, HttpTransportOptions options, RetryOptions retryOptions)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options ?? throw new ArgumentNullException(nameof(options));

        ConfigureHttpClient();
        _retryPolicy = CreateRetryPolicy(retryOptions);
        _jsonSettings = CreateJsonSettings();
    }

    /// <summary>
    /// Sends a single log entry to the Radianz backend
    /// </summary>
    public async Task SendLogEntryAsync(RadianzLogEntry logEntry, CancellationToken cancellationToken = default)
    {
        if (logEntry == null)
            throw new ArgumentNullException(nameof(logEntry));

        var json = JsonConvert.SerializeObject(logEntry, _jsonSettings);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        await ExecuteWithRetryAsync(async () =>
        {
            var response = await _httpClient.PostAsync(_options.LogEndpoint, content, cancellationToken);
            return response;
        }, cancellationToken);
    }

    /// <summary>
    /// Sends a batch of log entries to the Radianz backend
    /// </summary>
    public async Task SendLogBatchAsync(RadianzLogBatch logBatch, CancellationToken cancellationToken = default)
    {
        if (logBatch == null)
            throw new ArgumentNullException(nameof(logBatch));

        var json = JsonConvert.SerializeObject(logBatch, _jsonSettings);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        await ExecuteWithRetryAsync(async () =>
        {
            var response = await _httpClient.PostAsync($"{_options.LogEndpoint}/batch", content, cancellationToken);
            return response;
        }, cancellationToken);
    }

    /// <summary>
    /// Sends multiple log entries as a batch to the Radianz backend
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

    private void ConfigureHttpClient()
    {
        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);

        if (!string.IsNullOrWhiteSpace(_options.ClientId))
            _httpClient.DefaultRequestHeaders.Add("ClientId", _options.ClientId);

        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
            _httpClient.DefaultRequestHeaders.Add("ApiKey", _options.ApiKey);

        foreach (var header in _options.Headers)
            _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);

        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Serilog.Sinks.Radianz/1.0.0");

        var appVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString();
        if (!string.IsNullOrWhiteSpace(appVersion))
            _httpClient.DefaultRequestHeaders.Add("X-Application-Version", appVersion);
    }

    private IAsyncPolicy<HttpResponseMessage> CreateRetryPolicy(RetryOptions retryOptions)
    {
        var policyBuilder = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode && ShouldRetry(r.StatusCode))
            .Or<HttpRequestException>()
            .Or<TaskCanceledException>();

        if (retryOptions.UseExponentialBackoff)
        {
            var delays = Enumerable.Range(0, retryOptions.MaxRetryAttempts)
                .Select(attempt => TimeSpan.FromMilliseconds(
                    Math.Min(
                        retryOptions.BaseDelayMilliseconds * Math.Pow(2, attempt),
                        retryOptions.MaxDelayMilliseconds)));

            if (retryOptions.UseJitter)
            {
                var random = new Random();
                delays = delays.Select(delay =>
                    TimeSpan.FromMilliseconds(delay.TotalMilliseconds * (0.5 + random.NextDouble() * 0.5)));
            }

            return policyBuilder.WaitAndRetryAsync(delays);
        }
        else
        {
            var delay = TimeSpan.FromMilliseconds(retryOptions.BaseDelayMilliseconds);
            return policyBuilder.WaitAndRetryAsync(retryOptions.MaxRetryAttempts, _ => delay);
        }
    }

    private static bool ShouldRetry(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.RequestTimeout => true,
            HttpStatusCode.InternalServerError => true,
            HttpStatusCode.BadGateway => true,
            HttpStatusCode.ServiceUnavailable => true,
            HttpStatusCode.GatewayTimeout => true,
            HttpStatusCode.TooManyRequests => true,
            _ => false
        };
    }

    private async Task<HttpResponseMessage> ExecuteWithRetryAsync(
        Func<Task<HttpResponseMessage>> operation,
        CancellationToken cancellationToken)
    {
        var response = await _retryPolicy.ExecuteAsync(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await operation();
        });

        response.EnsureSuccessStatusCode();
        return response;
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
    /// Disposes the HTTP client
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _httpClient?.Dispose();
            _disposed = true;
        }
    }
}

