namespace GenericReportGenerator.Infrastructure.WeatherReports;

public record WeatherDataPoint
{
    public DateOnly Date { get; init; }

    public double MaxTemperature { get; init; }
}
