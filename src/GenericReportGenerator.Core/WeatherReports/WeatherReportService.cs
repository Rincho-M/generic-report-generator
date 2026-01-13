using GenericReportGenerator.Infrastructure;
using GenericReportGenerator.Infrastructure.WeatherReports;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace GenericReportGenerator.Core.WeatherReports;

public class WeatherReportService
{
    private readonly ApiDbContext _dbContext;
    private readonly IPublishEndpoint _busPublishEndpoint;
    private readonly IWeatherDataRepository _weatherRepository;
    private readonly IWeatherReportFileRepository _reportFileRepository;
    private readonly WeatherReportFileBuilder _reportFileBuilder;

    public WeatherReportService(
        ApiDbContext dbContext, 
        IPublishEndpoint busPublishEndpoint,
        IWeatherDataRepository weatherRepository,
        IWeatherReportFileRepository reportFileRepository,
        WeatherReportFileBuilder reportBuidler)
    {
        _dbContext = dbContext;
        _busPublishEndpoint = busPublishEndpoint;
        _weatherRepository = weatherRepository;
        _reportFileRepository = reportFileRepository;
        _reportFileBuilder = reportBuidler;
    }

    public async Task<WeatherReport> QueueReport(string city, DateOnly fromDate, DateOnly toDate, CancellationToken ct)
    {
        // Add report to db.
        WeatherReport reportRequest = new()
        {
            Id = Guid.CreateVersion7(),
            Status = ReportStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow,
            City = city,
            FromDate = fromDate,
            ToDate = toDate,
        };
        _dbContext.WeatherReports.Add(reportRequest);
        await _dbContext.SaveChangesAsync(ct);

        // Add request to message bus.
        CreateWeatherReportMessage reportMessage = new()
        {
            Id = reportRequest.Id,
            City = reportRequest.City,
            FromDate = reportRequest.FromDate,
            ToDate = reportRequest.ToDate,
        };
        await _busPublishEndpoint.Publish(reportMessage, ct);

        return reportRequest;
    }

    public async Task CreateReport(Guid reportId, CancellationToken ct)
    {
        WeatherReport report = await _dbContext.WeatherReports.SingleAsync(r => r.Id == reportId);

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
