using GenericReportGenerator.Infrastructure.Features.WeatherReports;

namespace GenericReportGenerator.Api.Features.WeatherReports.CreateReport;

public record CreateReportResponse
{
    public Guid Id { get; init; }

    public ReportStatus Status { get; init; }
}
