using FluentAssertions;
using Serilog.Events;
using Serilog.Parsing;
using Serilog.Sinks.Radianz.Configuration;
using Serilog.Sinks.Radianz.Formatting;
using Xunit;

namespace Serilog.Sinks.Radianz.Tests.Formatting;

public class DefaultRadianzLogEventFormatterTests
{
    private readonly FormattingOptions _defaultOptions = new();
    private readonly DefaultRadianzLogEventFormatter _formatter;
    private readonly MessageTemplateParser _parser = new();

    public DefaultRadianzLogEventFormatterTests()
    {
        _formatter = new DefaultRadianzLogEventFormatter(_defaultOptions);
    }

    [Fact]
    public void Constructor_WithNullOptions_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DefaultRadianzLogEventFormatter(null!));
    }

    [Fact]
    public void FormatLogEvent_WithNullLogEvent_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _formatter.FormatLogEvent(null!));
    }

    [Fact]
    public void FormatLogEvent_WithBasicLogEvent_ShouldFormatCorrectly()
    {
        // Arrange
        var timestamp = DateTimeOffset.UtcNow;
        var template = _parser.Parse("Hello {Name}");
        var logEvent = new LogEvent(
            timestamp,
            LogEventLevel.Information,
            null,
            template,
            new[] { new LogEventProperty("Name", new ScalarValue("World")) }
        );

        // Act
        var result = _formatter.FormatLogEvent(logEvent);

        // Assert
        result.Should().NotBeNull();
        result.Timestamp.Should().Be(timestamp.UtcDateTime);
        result.Level.Should().Be("Information");
        result.MessageTemplate.Should().Be("Hello {Name}");
        result.Message.Should().Be("Hello \"World\"");
        result.Properties.Should().ContainKey("Name");
        result.Properties["Name"].Should().Be("World");
    }

    [Fact]
    public void FormatLogEvent_WithException_ShouldIncludeExceptionDetails()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception", new ArgumentException("Inner exception"));
        var timestamp = DateTimeOffset.UtcNow;
        var template = _parser.Parse("An error occurred");
        var logEvent = new LogEvent(
            timestamp,
            LogEventLevel.Error,
            exception,
            template,
            Array.Empty<LogEventProperty>()
        );

        // Act
        var result = _formatter.FormatLogEvent(logEvent);

        // Assert
        result.Exception.Should().NotBeNull();
        result.Exception!.Type.Should().Be("System.InvalidOperationException");
        result.Exception.Message.Should().Be("Test exception");
        result.Exception.InnerException.Should().NotBeNull();
        result.Exception.InnerException!.Type.Should().Be("System.ArgumentException");
        result.Exception.InnerException.Message.Should().Be("Inner exception");
    }

    [Fact]
    public void FormatLogEvent_WithExceptionDetailsDisabled_ShouldNotIncludeException()
    {
        // Arrange
        var options = new FormattingOptions { IncludeExceptionDetails = false };
        var formatter = new DefaultRadianzLogEventFormatter(options);

        var exception = new InvalidOperationException("Test exception");
        var timestamp = DateTimeOffset.UtcNow;
        var template = _parser.Parse("An error occurred");
        var logEvent = new LogEvent(
            timestamp,
            LogEventLevel.Error,
            exception,
            template,
            Array.Empty<LogEventProperty>()
        );

        // Act
        var result = formatter.FormatLogEvent(logEvent);

        // Assert
        result.Exception.Should().BeNull();
    }

    [Fact]
    public void FormatLogEvent_WithMaxMessageLength_ShouldTruncateMessage()
    {
        // Arrange
        var options = new FormattingOptions { MaxMessageLength = 10 };
        var formatter = new DefaultRadianzLogEventFormatter(options);

        var timestamp = DateTimeOffset.UtcNow;
        var template = _parser.Parse("This is a very long message that should be truncated");
        var logEvent = new LogEvent(
            timestamp,
            LogEventLevel.Information,
            null,
            template,
            Array.Empty<LogEventProperty>()
        );

        // Act
        var result = formatter.FormatLogEvent(logEvent);

        // Assert
        result.Message.Should().Be("This is a ...");
        result.Message.Length.Should().Be(13); // 10 + "..."
    }

    [Fact]
    public void FormatLogEvent_WithSourceContext_ShouldExtractSourceContext()
    {
        // Arrange
        var timestamp = DateTimeOffset.UtcNow;
        var template = _parser.Parse("Test message");
        var logEvent = new LogEvent(
            timestamp,
            LogEventLevel.Information,
            null,
            template,
            new[] { new LogEventProperty("SourceContext", new ScalarValue("MyNamespace.MyClass")) }
        );

        // Act
        var result = _formatter.FormatLogEvent(logEvent);

        // Assert
        result.SourceContext.Should().Be("MyNamespace.MyClass");
    }

    [Fact]
    public void FormatLogEvent_WithAdditionalProperties_ShouldIncludeAdditionalProperties()
    {
        // Arrange
        var options = new FormattingOptions();
        options.AdditionalProperties["Environment"] = "Test";
        options.AdditionalProperties["Version"] = "1.0.0";

        var formatter = new DefaultRadianzLogEventFormatter(options);
        var timestamp = DateTimeOffset.UtcNow;
        var template = _parser.Parse("Test message");
        var logEvent = new LogEvent(
            timestamp,
            LogEventLevel.Information,
            null,
            template,
            Array.Empty<LogEventProperty>()
        );

        // Act
        var result = formatter.FormatLogEvent(logEvent);

        // Assert
        result.Metadata.Should().ContainKey("Environment");
        result.Metadata["Environment"].Should().Be("Test");
        result.Metadata.Should().ContainKey("Version");
        result.Metadata["Version"].Should().Be("1.0.0");
    }

    [Fact]
    public void FormatLogEvents_WithMultipleEvents_ShouldFormatAll()
    {
        // Arrange
        var timestamp = DateTimeOffset.UtcNow;
        var template1 = _parser.Parse("Message 1");
        var template2 = _parser.Parse("Message 2");
        var template3 = _parser.Parse("Message 3");
        var logEvents = new[]
        {
            new LogEvent(timestamp, LogEventLevel.Information, null, template1, Array.Empty<LogEventProperty>()),
            new LogEvent(timestamp, LogEventLevel.Warning, null, template2, Array.Empty<LogEventProperty>()),
            new LogEvent(timestamp, LogEventLevel.Error, null, template3, Array.Empty<LogEventProperty>())
        };

        // Act
        var results = _formatter.FormatLogEvents(logEvents).ToList();

        // Assert
        results.Should().HaveCount(3);
        results[0].Level.Should().Be("Information");
        results[0].Message.Should().Be("Message 1");
        results[1].Level.Should().Be("Warning");
        results[1].Message.Should().Be("Message 2");
        results[2].Level.Should().Be("Error");
        results[2].Message.Should().Be("Message 3");
    }

    [Fact]
    public void FormatLogEvents_WithNullLogEvents_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _formatter.FormatLogEvents(null!));
    }
}

