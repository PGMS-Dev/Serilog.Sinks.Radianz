using FluentAssertions;
using Serilog.Sinks.Radianz.Configuration;
using Xunit;

namespace Serilog.Sinks.Radianz.Tests.Configuration;

public class RadianzSinkOptionsTests
{
    [Fact]
    public void Validate_WithValidHttpOptions_ShouldNotThrow()
    {
        // Arrange
        var options = new RadianzSinkOptions
        {
            TransportType = RadianzTransportType.Http,
            HttpOptions = new HttpTransportOptions
            {
                BaseUrl = "https://api.radianz.io"
            }
        };

        // Act & Assert
        options.Invoking(o => o.Validate()).Should().NotThrow();
    }

    [Fact]
    public void Validate_WithValidMessageQueueOptions_ShouldNotThrow()
    {
        // Arrange
        var options = new RadianzSinkOptions
        {
            TransportType = RadianzTransportType.MessageQueue,
            MessageQueueOptions = new MessageQueueTransportOptions
            {
                ConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=test",
                QueueName = "logs"
            }
        };

        // Act & Assert
        options.Invoking(o => o.Validate()).Should().NotThrow();
    }

    [Fact]
    public void Validate_WithHttpTransportButNoHttpOptions_ShouldThrow()
    {
        // Arrange
        var options = new RadianzSinkOptions
        {
            TransportType = RadianzTransportType.Http,
            HttpOptions = null
        };

        // Act & Assert
        options.Invoking(o => o.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("HttpOptions must be configured when using HTTP transport");
    }

    [Fact]
    public void Validate_WithMessageQueueTransportButNoMessageQueueOptions_ShouldThrow()
    {
        // Arrange
        var options = new RadianzSinkOptions
        {
            TransportType = RadianzTransportType.MessageQueue,
            MessageQueueOptions = null
        };

        // Act & Assert
        options.Invoking(o => o.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("MessageQueueOptions must be configured when using MessageQueue transport");
    }
}

public class HttpTransportOptionsTests
{
    [Fact]
    public void Validate_WithValidBaseUrl_ShouldNotThrow()
    {
        // Arrange
        var options = new HttpTransportOptions
        {
            BaseUrl = "https://api.radianz.io"
        };

        // Act & Assert
        options.Invoking(o => o.Validate()).Should().NotThrow();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithInvalidBaseUrl_ShouldThrow(string? baseUrl)
    {
        // Arrange
        var options = new HttpTransportOptions
        {
            BaseUrl = baseUrl!
        };

        // Act & Assert
        options.Invoking(o => o.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("BaseUrl is required for HTTP transport");
    }

    [Fact]
    public void Validate_WithInvalidUri_ShouldThrow()
    {
        // Arrange
        var options = new HttpTransportOptions
        {
            BaseUrl = "not-a-valid-uri"
        };

        // Act & Assert
        options.Invoking(o => o.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("BaseUrl must be a valid absolute URI");
    }

    [Fact]
    public void Validate_WithZeroTimeout_ShouldThrow()
    {
        // Arrange
        var options = new HttpTransportOptions
        {
            BaseUrl = "https://api.radianz.io",
            TimeoutSeconds = 0
        };

        // Act & Assert
        options.Invoking(o => o.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("TimeoutSeconds must be greater than 0");
    }
}

public class MessageQueueTransportOptionsTests
{
    [Fact]
    public void Validate_WithValidOptions_ShouldNotThrow()
    {
        // Arrange
        var options = new MessageQueueTransportOptions
        {
            ConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=test",
            QueueName = "logs"
        };

        // Act & Assert
        options.Invoking(o => o.Validate()).Should().NotThrow();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithInvalidConnectionString_ShouldThrow(string? connectionString)
    {
        // Arrange
        var options = new MessageQueueTransportOptions
        {
            ConnectionString = connectionString!,
            QueueName = "logs"
        };

        // Act & Assert
        options.Invoking(o => o.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("ConnectionString is required for MessageQueue transport");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithInvalidQueueName_ShouldThrow(string? queueName)
    {
        // Arrange
        var options = new MessageQueueTransportOptions
        {
            ConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=test",
            QueueName = queueName!
        };

        // Act & Assert
        options.Invoking(o => o.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("QueueName is required for MessageQueue transport");
    }

    [Fact]
    public void Validate_WithZeroMessageTtl_ShouldThrow()
    {
        // Arrange
        var options = new MessageQueueTransportOptions
        {
            ConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=test",
            QueueName = "logs",
            MessageTtlMinutes = 0
        };

        // Act & Assert
        options.Invoking(o => o.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("MessageTtlMinutes must be greater than 0");
    }
}

