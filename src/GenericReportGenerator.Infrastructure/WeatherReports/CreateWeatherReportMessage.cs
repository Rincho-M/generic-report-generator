namespace GenericReportGenerator.Infrastructure.WeatherReports;

public record CreateWeatherReportMessage
{
    public required Guid Id { get; init; }

    public required string City { get; init; }

    public required DateOnly FromDate { get; init; }

    public required DateOnly ToDate { get; init; }
}
