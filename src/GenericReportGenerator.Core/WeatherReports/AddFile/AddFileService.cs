using GenericReportGenerator.Infrastructure;
using GenericReportGenerator.Infrastructure.WeatherReports;
using GenericReportGenerator.Infrastructure.WeatherReports.ReportFiles;
using GenericReportGenerator.Infrastructure.WeatherReports.WeatherData;
using Microsoft.EntityFrameworkCore;

namespace GenericReportGenerator.Core.WeatherReports.AddFile;

/// <summary>
/// Service for adding a generated weather report file to a corresponding report entry in database.
/// </summary>
public class AddFileService
{
    private readonly AppDbContext _dbContext;
    private readonly IWeatherDataRepository _weatherRepository;
    private readonly IReportFileRepository _reportFileRepository;
    private readonly ReportFileBuilder _reportFileBuilder;

    public AddFileService(
        AppDbContext dbContext,
        IWeatherDataRepository weatherRepository,
        IReportFileRepository reportFileRepository,
        ReportFileBuilder reportBuidler)
    {
        _dbContext = dbContext;
        _weatherRepository = weatherRepository;
        _reportFileRepository = reportFileRepository;
        _reportFileBuilder = reportBuidler;
    }

    public async Task AddFileToReport(Guid reportId, CancellationToken ct)
    {
        Report report = await _dbContext.WeatherReports
            .SingleAsync(r => r.Id == reportId, ct);

        IReadOnlyCollection<WeatherDataPoint> weatherData =
            await _weatherRepository.GetWeatherHistory(report.City, report.FromDate, report.ToDate, ct);

        Stream reportFile = _reportFileBuilder.Build(report.Id, report.City, report.FromDate, report.ToDate, weatherData);
        string reportFilePath = await _reportFileRepository.Save(report.Id, reportFile);

        report.Status = ReportStatus.Completed;
        report.CompletedAt = DateTimeOffset.UtcNow;
        report.FilePath = reportFilePath;

        await _dbContext.SaveChangesAsync(ct);
    }
}
