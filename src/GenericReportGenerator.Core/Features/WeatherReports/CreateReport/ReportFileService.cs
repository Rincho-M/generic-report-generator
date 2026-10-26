using GenericReportGenerator.Infrastructure.Features.WeatherReports;
using GenericReportGenerator.Infrastructure.Features.WeatherReports.ReportFiles;
using GenericReportGenerator.Infrastructure.Features.WeatherReports.WeatherData;

namespace GenericReportGenerator.Core.Features.WeatherReports.CreateReport;

/// <summary>
/// Generates a weather report Excel file and saves it to file storage.
/// </summary>
public class ReportFileService
{
    private readonly IWeatherDataRepository _weatherRepository;
    private readonly IReportFileRepository _reportFileRepository;
    private readonly ReportFileBuilder _reportFileBuilder;

    public ReportFileService(
        IWeatherDataRepository weatherRepository,
        IReportFileRepository reportFileRepository,
        ReportFileBuilder reportFileBuilder)
    {
        _weatherRepository = weatherRepository;
        _reportFileRepository = reportFileRepository;
        _reportFileBuilder = reportFileBuilder;
    }

    public async Task<string> GenerateAndSave(Report report, CancellationToken ct)
    {
        IReadOnlyCollection<WeatherDataPoint> weatherData =
            await _weatherRepository.GetWeatherHistory(report.City, report.FromDate, report.ToDate, ct);

        Stream reportFile = _reportFileBuilder.Build(report.Id, report.City, report.FromDate, report.ToDate, weatherData);
        string reportFilePath = await _reportFileRepository.Save(report.Id, reportFile);

        return reportFilePath;
    }
}
