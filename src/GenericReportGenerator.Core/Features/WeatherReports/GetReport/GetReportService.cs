using GenericReportGenerator.Infrastructure.Common;
using GenericReportGenerator.Infrastructure.Features.WeatherReports;
using Microsoft.EntityFrameworkCore;

namespace GenericReportGenerator.Core.Features.WeatherReports.GetReport;

public class GetReportService
{
    private readonly AppDbContext _dbContext;

    public GetReportService(
        AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Report> GetReport(Guid id, CancellationToken ct)
    {
        Report report = await _dbContext.WeatherReports.SingleAsync(report => report.Id == id, ct);

        return report;
    }
}
