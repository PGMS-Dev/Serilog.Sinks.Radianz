# Serilog.Sinks.Radianz

A Serilog sink that sends log events to your Radianz instance for centralized logging and observability.

**[Radianz](https://radianz.io)** is a developer activity analytics and observability platform that helps teams understand their development workflow and application behavior.

## Features

- **Multiple Transport Options**: Send logs via HTTP REST API or Azure Service Bus message queue
- **Async Batching**: Efficient batching of log events for optimal performance
- **Retry Logic**: Built-in retry policies with exponential backoff and jitter
- **Configurable Formatting**: Customizable log event formatting
- **Error Resilience**: Robust error handling that won't break your application
- **Structured Logging**: Full support for structured logging with properties and scopes
- **.NET 10 Support**: Built for the latest .NET runtime

## Quick Start

### HTTP Transport (Recommended)

```csharp
using Serilog;
using Serilog.Sinks.Radianz.Extensions;

var logger = new LoggerConfiguration()
    .WriteTo.RadianzHttp(
        baseUrl: "https://your-radianz-instance.com",
        clientId: "your-client-id",
        apiKey: "your-api-key")
    .CreateLogger();

logger.Information("Application started");
```

### Message Queue Transport

```csharp
using Serilog;
using Serilog.Sinks.Radianz.Extensions;

var logger = new LoggerConfiguration()
    .WriteTo.RadianzMessageQueue(
        connectionString: "Endpoint=sb://your-namespace.servicebus.windows.net/...",
        queueName: "radianz-logs")
    .CreateLogger();
```

## License

This project is licensed under the MIT License.

