using GenericReportGenerator.Infrastructure.Common;
using GenericReportGenerator.Infrastructure.Features.WeatherReports;
using Microsoft.EntityFrameworkCore;

namespace GenericReportGenerator.Core.Features.WeatherReports.CreateReport;

/// <summary>
/// Orchestrates worker-side weather report processing and status transitions
/// (InProgress, Completed, Failed). Pending is set when the report is created.
/// </summary>
public class ReportProcessingService
{
    private readonly AppDbContext _dbContext;
    private readonly ReportFileService _reportFileService;

    public ReportProcessingService(
        AppDbContext dbContext,
        ReportFileService reportFileService)
    {
        _dbContext = dbContext;
        _reportFileService = reportFileService;
    }

    public async Task Process(Guid reportId, CancellationToken ct)
    {
        Report report = await _dbContext.WeatherReports
            .SingleAsync(r => r.Id == reportId, ct);

        if (report.Status is ReportStatus.Completed or ReportStatus.Failed)
        {
            return;
        }

        report.Status = ReportStatus.InProgress;
        await _dbContext.SaveChangesAsync(ct);

        string reportFilePath = await _reportFileService.GenerateAndSave(report, ct);

        report.Status = ReportStatus.Completed;
        report.CompletedAt = DateTimeOffset.UtcNow;
        report.FilePath = reportFilePath;

        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task MarkFailed(Guid reportId, CancellationToken ct)
    {
        Report report = await _dbContext.WeatherReports
            .SingleAsync(r => r.Id == reportId, ct);

        if (report.Status is ReportStatus.Completed or ReportStatus.Failed)
        {
            return;
        }

        report.Status = ReportStatus.Failed;
        await _dbContext.SaveChangesAsync(ct);
    }
}
