# Serilog.Sinks.Radianz

A [Serilog](https://serilog.net/) sink for **[Radianz](https://radianz.io)** — developer activity analytics & observability.

Supports **.NET 8** and **.NET 10**.

## Quick Start

```csharp
using Serilog;
using Serilog.Sinks.Radianz.Extensions;

Log.Logger = new LoggerConfiguration()
    .WriteTo.RadianzHttp(
        clientId: "your-client-id",
        apiKey: "your-api-key")
    .CreateLogger();
```

Async batching • Retry with back-off • Structured logging • HTTP & Azure Service Bus transports.

Full documentation: [github.com/PGMS-Dev/Serilog.Sinks.Radianz](https://github.com/PGMS-Dev/Serilog.Sinks.Radianz)
