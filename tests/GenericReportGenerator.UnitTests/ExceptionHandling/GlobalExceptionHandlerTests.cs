using System.Diagnostics;
using System.Text.Json;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using GenericReportGenerator.Api.ExceptionHandling;
using GenericReportGenerator.Infrastructure.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace GenericReportGenerator.UnitTests.ExceptionHandling;

public class GlobalExceptionHandlerTests
{
    private GlobalExceptionHandler _handler;

    private ILogger<GlobalExceptionHandler> _logger;

    private DefaultHttpContext _httpContext;

    private MemoryStream _responseStream;

    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILogger<GlobalExceptionHandler>>();
        _handler = new GlobalExceptionHandler(_logger);

        _httpContext = new DefaultHttpContext();
        _responseStream = new MemoryStream();
        _httpContext.Response.Body = _responseStream;
    }

    [TearDown]
    public void TearDown()
    {
        _responseStream.Dispose();
        Activity.Current = null;
    }

    [Test]
    public async Task ShouldReturn400_WhenValidationExceptionThrown()
    {
        // Arrange.
        List<ValidationFailure> validationErrors = new()
        {
            new("City", "Required"),
            new("Date", "Invalid")
        };
        ValidationException exception = new(validationErrors);

        // Act.
        await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert.
        _httpContext.Response.StatusCode.Should().Be(400);

        ValidationErrorResponse? response = await ReadResponse<ValidationErrorResponse>();
        response.Should().NotBeNull();
        response.Name.Should().Be(nameof(ValidationException));
        response.Errors.Should().ContainKey("City").WhoseValue.Should().Contain("Required");
        response.Errors.Should().ContainKey("Date").WhoseValue.Should().Contain("Invalid");

        _logger.Received().Log(LogLevel.Information,
            Arg.Any<EventId>(), Arg.Any<object>(), Arg.Any<Exception>(), Arg.Any<Func<object, Exception?, string>>());
    }

    [Test]
    public async Task ShouldReturn422_WhenDomainExceptionThrown()
    {
        // Arrange.
        TestDomainException exception = new("Business Rule Broken");

        // Act.
        await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert.
        _httpContext.Response.StatusCode.Should().Be(422);

        DetailErrorResponse? response = await ReadResponse<DetailErrorResponse>();
        response.Should().NotBeNull();
        response.Name.Should().Be(nameof(TestDomainException));
        response.Details.Should().Be(exception.Message);

        _logger.Received().Log(LogLevel.Warning,
            Arg.Any<EventId>(), Arg.Any<object>(), Arg.Any<Exception>(), Arg.Any<Func<object, Exception?, string>>());
    }

    [Test]
    public async Task ShouldReturn500_WhenSystemExceptionThrown()
    {
        // Arrange.
        NullReferenceException exception = new("Test Null Reference");

        // Act.
        await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert.
        _httpContext.Response.StatusCode.Should().Be(500);

        DetailErrorResponse? response = await ReadResponse<DetailErrorResponse>();
        response.Should().NotBeNull();
        response.Name.Should().Be("SystemException");
        response.Details.Should().Be("Internal server error.");

        _logger.Received().Log(LogLevel.Error,
            Arg.Any<EventId>(), Arg.Any<object>(), Arg.Any<Exception>(), Arg.Any<Func<object, Exception?, string>>());
    }

    [Test]
    public async Task ShouldIncludeTraceId_WhenActivityPresent()
    {
        // Arrange.
        // Simulate an OpenTelemetry/ASP.NET Activity.
        Activity activity = new Activity("TestOperation").Start();
        activity.SetIdFormat(ActivityIdFormat.W3C);

        Exception exception = new();

        // Act.
        await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert.
        DetailErrorResponse? response = await ReadResponse<DetailErrorResponse>();
        response.Should().NotBeNull();
        response.TraceId.Should().Be(activity.Id);

        activity.Stop();
    }

    [Test]
    public async Task ShouldFallbackToContextTraceId_WhenActivityIsNull()
    {
        // Arrange.
        Activity.Current = null;
        _httpContext.TraceIdentifier = "fallback-trace-id";
        Exception exception = new();

        // Act.
        await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert.
        DetailErrorResponse? response = await ReadResponse<DetailErrorResponse>();
        response.Should().NotBeNull();
        response.TraceId.Should().Be("fallback-trace-id");
    }

    #region Helpers

    // Stub for Domain Exception.
    private class TestDomainException : DomainException
    {
        public TestDomainException(string message) : base(message) { }
    }

    // Helper to read and deserialize response.
    private async Task<T?> ReadResponse<T>()
    {
        _responseStream.Position = 0;
        JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };
        T? response = await JsonSerializer.DeserializeAsync<T>(_responseStream, options);

        return response;
    }

    #endregion
}
