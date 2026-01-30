using GenericReportGenerator.Infrastructure.Features.WeatherReports;

namespace GenericReportGenerator.Api.Features.WeatherReports.GetReport;

public record GetReportResponse
{
    public Guid Id { get; init; }

    public ReportStatus Status { get; init; }

    public required string City { get; init; }

    public DateOnly FromDate { get; init; }

    public DateOnly ToDate { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
}
