using GenericReportGenerator.Infrastructure.WeatherReports;

namespace GenericReportGenerator.Api.Features.WeatherReports.CreateReport;

public record CreateReportResponse
{
    public Guid ReportId { get; init; }

    public ReportStatus ReportStatus { get; init; }
}
