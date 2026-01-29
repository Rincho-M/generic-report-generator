using GenericReportGenerator.Core.Features.WeatherReports.GetFile;
using GenericReportGenerator.Infrastructure.Features.WeatherReports.ReportFiles;

namespace GenericReportGenerator.Api.Features.WeatherReports.GetReportFile;

public static class GetReportFileEndpoint
{
    private const string ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public static void Map(IEndpointRouteBuilder builder)
    {
        builder.MapGet("{id:guid}/file", GetReportFile)
            .WithName(nameof(GetReportFileEndpoint))
            .Produces<Stream>(StatusCodes.Status200OK, ContentType);
    }

    /// <summary>
    /// Get completed report file.
    /// </summary>
    /// <param name="id">Report id.</param>
    public static async Task<IResult> GetReportFile(
        Guid id,
        GetFileSerivce fileService,
        CancellationToken ct)
    {
        ReportFile file = await fileService.GetReportFile(id, ct);

        return Results.File(
            file.Content,
            contentType: ContentType,
            fileDownloadName: file.ReadableFileName
        );
    }
}
