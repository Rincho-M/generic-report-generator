using System.Text.Json;
using GenericReportGenerator.Infrastructure.Common;
using GenericReportGenerator.Infrastructure.Common.Outbox;
using GenericReportGenerator.Infrastructure.Features.WeatherReports.ReportFiles;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GenericReportGenerator.Core.Common.Outbox;

/// <summary>
/// Polls pending outbox rows and publishes them to the message bus after a successful commit.
/// </summary>
public class OutboxDispatcher : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly OutboxOptions _options;
    private readonly ILogger<OutboxDispatcher> _logger;

    public OutboxDispatcher(
        IServiceScopeFactory scopeFactory,
        IOptions<OutboxOptions> options,
        ILogger<OutboxDispatcher> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DispatchPending(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Outbox dispatch cycle failed.");
            }

            try
            {
                await Task.Delay(_options.PollingInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task DispatchPending(CancellationToken ct)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        IPublishEndpoint publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        List<OutboxMessage> pendingMessages = await dbContext.OutboxMessages
            .Where(message => message.ProcessedAt == null)
            .OrderBy(message => message.CreatedAt)
            .Take(_options.BatchSize)
            .ToListAsync(ct);

        foreach (OutboxMessage outboxMessage in pendingMessages)
        {
            try
            {
                await Publish(outboxMessage, publishEndpoint, ct);

                outboxMessage.ProcessedAt = DateTimeOffset.UtcNow;
                await dbContext.SaveChangesAsync(ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Failed to publish outbox message {OutboxMessageId} of type {MessageType}.",
                    outboxMessage.Id,
                    outboxMessage.Type);
            }
        }
    }

    private async Task Publish(OutboxMessage outboxMessage, IPublishEndpoint publishEndpoint, CancellationToken ct)
    {
        switch (outboxMessage.Type)
        {
            case nameof(CreateReportFileMessage):
                CreateReportFileMessage? message = JsonSerializer.Deserialize<CreateReportFileMessage>(outboxMessage.Payload);
                if (message is null)
                {
                    throw new JsonException($"Outbox payload for {nameof(CreateReportFileMessage)} deserialized to null.");
                }

                await publishEndpoint.Publish(message, ct);
                break;

            default:
                throw new InvalidOperationException($"Unsupported outbox message type '{outboxMessage.Type}'.");
        }
    }
}
