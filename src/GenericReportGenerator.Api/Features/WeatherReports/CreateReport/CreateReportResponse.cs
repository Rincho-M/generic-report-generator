using GenericReportGenerator.Infrastructure.WeatherReports;

namespace GenericReportGenerator.Api.Features.WeatherReports.CreateReport;

internal record CreateReportResponse
{
    public Guid RequestId { get; init; }

    public ReportStatus RequestStatus { get; init; }
}
