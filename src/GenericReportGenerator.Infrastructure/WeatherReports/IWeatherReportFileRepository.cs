namespace GenericReportGenerator.Infrastructure.WeatherReports;

public interface IWeatherReportFileRepository
{
    Task<string> Save(Guid reportId, Stream reportFile);
}
