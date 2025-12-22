using Microsoft.AspNetCore.Mvc;

namespace GenericReportGenerator.Api.Endpoints.WeatherReport;

internal static class WeatherReportEndpoints
{
    internal static void MapWeatherReportEndpoints(this IEndpointRouteBuilder builder)
    {
        RouteGroupBuilder group = builder.MapGroup("weather-report");

        group.MapPost(string.Empty, CreateReport);
        group.MapGet("status", GetReportStatus);
        group.MapGet("document", DownloadReportFile);
    }

    [ProducesResponseType(type: typeof(object), statusCode: 200)]
    internal static async Task<IResult> CreateReport(
        CancellationToken ct)
    {
        return Results.Json(null);
    }

    [ProducesResponseType(type: typeof(object), statusCode: 200)]
    internal static async Task<IResult> GetReportStatus(
        CancellationToken ct)
    {
        return Results.Json(null);
    }

    [ProducesResponseType(type: typeof(object), statusCode: 200)]
    internal static async Task<IResult> DownloadReportFile(
        CancellationToken ct)
    {
        return Results.Json(null);
    }
}
