# Serilog.Sinks.Radianz

[![NuGet](https://img.shields.io/nuget/v/Serilog.Sinks.Radianz.svg)](https://www.nuget.org/packages/Serilog.Sinks.Radianz)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

A [Serilog](https://serilog.net/) sink that sends structured log events to **[Radianz](https://radianz.io)** — a developer activity analytics and observability platform that gives engineering teams real-time visibility into their development workflow, application health, and production behavior.

## Installation

```shell
dotnet add package Serilog.Sinks.Radianz
```

> Supports **.NET 8** and **.NET 10** (multi-target).

## Quick Start

```csharp
using Serilog;
using Serilog.Sinks.Radianz.Extensions;

Log.Logger = new LoggerConfiguration()
    .WriteTo.RadianzHttp(
        clientId: "your-client-id",
        apiKey: "your-api-key")
    .CreateLogger();

Log.Information("Hello from {AppName}!", "MyApp");
```

That's it. Logs are batched and sent asynchronously to Radianz — zero impact on your app's performance.

## ASP.NET Core Integration

```csharp
builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.RadianzHttp(
        clientId: ctx.Configuration["Radianz:ClientId"],
        apiKey: ctx.Configuration["Radianz:ApiKey"])
    .WriteTo.Console());
```

## Features

| Feature | Description |
|---|---|
| **Async Batching** | Logs are buffered and sent in configurable batches (default 50 every 5 s) |
| **Retry with Back-off** | Exponential back-off + jitter via Polly — survives transient failures |
| **Structured Logging** | Full support for properties, scopes, and exception details |
| **HTTP & Message Queue** | Choose REST API (default) or Azure Service Bus transport |
| **Multi-target** | Single NuGet package for .NET 8 and .NET 10 |

## Advanced Configuration

### Custom Batching & Minimum Level

```csharp
.WriteTo.RadianzHttp(
    clientId: "your-client-id",
    apiKey: "your-api-key",
    restrictedToMinimumLevel: LogEventLevel.Warning,
    batchSizeLimit: 100,
    batchPeriod: TimeSpan.FromSeconds(10))
```

### Full Options via Action

```csharp
.WriteTo.Radianz(options =>
{
    options.TransportType = RadianzTransportType.Http;
    options.HttpOptions = new HttpTransportOptions
    {
        ClientId = "your-client-id",
        ApiKey   = "your-api-key"
    };
    options.Batching.BatchSizeLimit = 100;
    options.Retry.MaxRetryAttempts  = 5;
})
```

### Azure Service Bus Transport

```csharp
.WriteTo.RadianzMessageQueue(
    connectionString: "Endpoint=sb://your-ns.servicebus.windows.net/;...",
    queueName: "radianz-logs")
```

## Configuration Reference

| Parameter | Default | Description |
|---|---|---|
| `clientId` | — | Your Radianz client identifier |
| `apiKey` | — | Your Radianz API key |
| `restrictedToMinimumLevel` | `Verbose` | Minimum log level to send |
| `batchSizeLimit` | `50` | Max events per batch |
| `batchPeriod` | `5 s` | Max wait before flushing a batch |
| `queueLimit` | `10 000` | Max queued events before dropping |

## License

[MIT](LICENSE)