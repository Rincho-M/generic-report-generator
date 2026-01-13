using FluentValidation;
using FluentValidation.Results;
using GenericReportGenerator.Api.Features.WeatherReports.GetReportStatus;
using GenericReportGenerator.Core.WeatherReports;
using GenericReportGenerator.Infrastructure.WeatherReports;

namespace GenericReportGenerator.Api.Features.WeatherReports.CreateReport;

internal static class CreateReportEndpoint
{
    internal static void Map(IEndpointRouteBuilder builder)
    {
        builder.MapPost(string.Empty, CreateReport);
    }

    /// <summary>
    /// Start a process of creating a weather report by putting a request in the queue.
    /// </summary>
    internal static async Task<IResult> CreateReport(
        CreateReportRequest request,
        IValidator<CreateReportRequest> validator,
        WeatherReportService reportService,
        CancellationToken ct)
    {
        ValidationResult validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        WeatherReport reportRequest = await reportService.QueueReport(request.City, request.FromDate, request.ToDate, ct);
        CreateReportResponse responseData = new() { RequestId = reportRequest.Id, RequestStatus = reportRequest.Status };

        return TypedResults.AcceptedAtRoute(
            routeName: nameof(GetReportStatusEndpoint),
            routeValues: new { id = responseData.RequestId },
            value: responseData);
    }
}
