using GenericReportGenerator.Core.WeatherReports.GetFile;
using GenericReportGenerator.Infrastructure.WeatherReports.ReportFiles;

namespace GenericReportGenerator.Api.Features.WeatherReports.GetReportFile;

public static class GetReportFileEndpoint
{
    private const string ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public static void Map(IEndpointRouteBuilder builder)
    {
        builder.MapGet("{reportId:guid}/file", GetReportFile);
    }

    private static async Task<IResult> GetReportFile(
        Guid reportId,
        GetFileSerivce fileService,
        CancellationToken ct)
    {
        ReportFile file = await fileService.GetReportFile(reportId, ct);

        return Results.File(
            file.Content,
            contentType: ContentType,
            fileDownloadName: file.ReadableFileName
        );
    }
}
