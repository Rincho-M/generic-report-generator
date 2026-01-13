using Microsoft.AspNetCore.Mvc;

namespace GenericReportGenerator.Api.Features.WeatherReports.GetReportFile;

internal static class GetReportFileEndpoint
{
    internal static void Map(IEndpointRouteBuilder builder)
    {
        builder.MapGet("file", GetReportFile);
    }

    [ProducesResponseType(type: typeof(object), statusCode: 200)]
    private static async Task<IResult> GetReportFile(
        CancellationToken ct)
    {
        return Results.Json(null);
    }
}
