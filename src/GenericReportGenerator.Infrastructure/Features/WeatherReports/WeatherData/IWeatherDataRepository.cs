namespace GenericReportGenerator.Infrastructure.Features.WeatherReports.WeatherData;

public interface IWeatherDataRepository
{
    Task<IReadOnlyCollection<WeatherDataPoint>> GetWeatherHistory(string city, DateOnly fromDate, DateOnly toDate, CancellationToken ct);
}
