using GenericReportGenerator.Infrastructure;
using GenericReportGenerator.Infrastructure.WeatherReports;
using GenericReportGenerator.Infrastructure.WeatherReports.ReportFiles;
using Microsoft.EntityFrameworkCore;

namespace GenericReportGenerator.Core.WeatherReports.GetFile;

public class GetFileSerivce
{
    private readonly AppDbContext _dbContext;

    private readonly IReportFileRepository _reportFileRepository;

    private const string _readableNameFormat = "WeatherReport_{0}_{1:yyyyMMdd}_{2:yyyyMMdd}.xlsx";

    public GetFileSerivce(
        AppDbContext dbContext,
        IReportFileRepository reportFileRepository)
    {
        _dbContext = dbContext;
        _reportFileRepository = reportFileRepository;
    }

    public async Task<ReportFile> GetReportFile(Guid reportId, CancellationToken ct)
    {
        Report report = await _dbContext.WeatherReports
            .AsNoTracking()
            .SingleAsync(report => report.Id == reportId, ct);

        ReportFile file = await _reportFileRepository.GetByReportId(reportId);

        file.ReadableFileName = string.Format(_readableNameFormat, report.City, report.FromDate, report.ToDate);

        return file;
    }
}
