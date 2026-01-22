using FluentValidation;
using GenericReportGenerator.Core.WeatherReports.QueueReport;
using GenericReportGenerator.Infrastructure.WeatherReports;
using GenericReportGenerator.Infrastructure.WeatherReports.WeatherData.Exceptions;

namespace GenericReportGenerator.Api.Features.WeatherReports.CreateReport;

public static class CreateReportEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder
            .MapPost(string.Empty, CreateReport)
            .WithName(nameof(CreateReportEndpoint));
    }

    /// <summary>
    /// Start a process of creating a weather report by putting a request in the queue.
    /// </summary>
    public static async Task<IResult> CreateReport(
        CreateReportRequest request,
        IValidator<CreateReportRequest> validator,
        QueueReportService reportService,
        CancellationToken ct)
    {
        await validator.ValidateAndThrowAsync(request, ct);

        Report reportRequest = await reportService.QueueReport(request.City, request.FromDate, request.ToDate, ct);
        CreateReportResponse responseData = new() { ReportId = reportRequest.Id, ReportStatus = reportRequest.Status };

        return Results.Ok(responseData);
    }
}
