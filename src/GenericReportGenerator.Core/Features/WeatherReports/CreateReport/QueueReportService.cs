using GenericReportGenerator.Infrastructure.Common;
using GenericReportGenerator.Infrastructure.Features.WeatherReports;
using GenericReportGenerator.Infrastructure.Features.WeatherReports.ReportFiles;
using MassTransit;

namespace GenericReportGenerator.Core.Features.WeatherReports.CreateReport;

/// <summary>
/// Service for queuing weather report generation requests that later asynchronously processed by worker service.
/// </summary>
public class QueueReportService
{
    private readonly AppDbContext _dbContext;
    private readonly IPublishEndpoint _busPublishEndpoint;

    public QueueReportService(
        AppDbContext dbContext,
        IPublishEndpoint busPublishEndpoint)
    {
        _dbContext = dbContext;
        _busPublishEndpoint = busPublishEndpoint;
    }

    public async Task<Report> QueueReport(string city, DateOnly fromDate, DateOnly toDate, CancellationToken ct)
    {
        // Add report without file to db.
        Report report = new()
        {
            Id = Guid.CreateVersion7(),
            Status = ReportStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow,
            City = city,
            FromDate = fromDate,
            ToDate = toDate,
        };
        _dbContext.WeatherReports.Add(report);
        await _dbContext.SaveChangesAsync(ct);

        // Add report request to message bus.
        CreateReportFileMessage reportMessage = new()
        {
            ReportId = report.Id
        };
        await _busPublishEndpoint.Publish(reportMessage, ct);

        return report;
    }
}
