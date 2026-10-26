using System.Text.Json;
using GenericReportGenerator.Infrastructure.Common;
using GenericReportGenerator.Infrastructure.Common.Outbox;
using GenericReportGenerator.Infrastructure.Features.WeatherReports;
using GenericReportGenerator.Infrastructure.Features.WeatherReports.ReportFiles;

namespace GenericReportGenerator.Core.Features.WeatherReports.CreateReport;

/// <summary>
/// Service for queuing weather report generation requests that later asynchronously processed by worker service.
/// </summary>
public class QueueReportService
{
    private readonly AppDbContext _dbContext;

    public QueueReportService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Report> QueueReport(string city, DateOnly fromDate, DateOnly toDate, CancellationToken ct)
    {
        DateTimeOffset currentDateTime = DateTimeOffset.UtcNow;

        Report report = new()
        {
            Id = Guid.CreateVersion7(),
            Status = ReportStatus.Pending,
            CreatedAt = currentDateTime,
            City = city,
            FromDate = fromDate,
            ToDate = toDate,
        };

        OutboxMessage outboxMessage = new()
        {
            Id = Guid.CreateVersion7(),
            Type = nameof(CreateReportFileMessage),
            Payload = JsonSerializer.Serialize(new CreateReportFileMessage { ReportId = report.Id }),
            CreatedAt = currentDateTime,
        };

        // Add report without file and outbox message to db.
        _dbContext.WeatherReports.Add(report);
        _dbContext.OutboxMessages.Add(outboxMessage);
        await _dbContext.SaveChangesAsync(ct);

        return report;
    }
}
