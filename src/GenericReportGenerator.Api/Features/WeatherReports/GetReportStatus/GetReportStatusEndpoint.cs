using Microsoft.AspNetCore.Mvc;

namespace GenericReportGenerator.Api.Features.WeatherReports.GetReportStatus;

internal static class GetReportStatusEndpoint
{
    internal static void Map(IEndpointRouteBuilder builder)
    {
        builder
            .MapGet("status", GetReportStatus)
            .WithName(nameof(GetReportStatusEndpoint));
    }

    [ProducesResponseType(type: typeof(object), statusCode: 200)]
    private static async Task<IResult> GetReportStatus(
        CancellationToken ct)
    {
        return Results.Json(null);
    }
}
