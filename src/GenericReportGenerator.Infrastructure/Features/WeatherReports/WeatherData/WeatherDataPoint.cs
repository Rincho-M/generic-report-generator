namespace GenericReportGenerator.Infrastructure.Features.WeatherReports.WeatherData;

/// <summary>
/// Single data point of weather information.
/// </summary>
public readonly record struct WeatherDataPoint
{
    public DateOnly Date { get; init; }

    public double MaxTemperature { get; init; }
}
