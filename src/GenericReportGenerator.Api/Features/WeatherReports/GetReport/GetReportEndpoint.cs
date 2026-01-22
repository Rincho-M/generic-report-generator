using GenericReportGenerator.Core.WeatherReports.GetReport;
using GenericReportGenerator.Infrastructure.WeatherReports;
using GenericReportGenerator.Infrastructure.WeatherReports.WeatherData.Exceptions;

namespace GenericReportGenerator.Api.Features.WeatherReports.GetReport;

public static class GetReportEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder
            .MapGet("{id:guid}", GetReport)
            .WithName(nameof(GetReportEndpoint));
    }

    private static async Task<IResult> GetReport(
        Guid id,
        GetReportService reportService,
        CancellationToken ct)
    {
        Report report = await reportService.GetReport(id, ct);

        GetReportResponse response = new()
        {
            Id = report.Id,
            Status = report.Status,
            City = report.City,
            FromDate = report.FromDate,
            ToDate = report.ToDate,
            CreatedAt = report.CreatedAt
        };

        return Results.Ok(response);
    }
}
