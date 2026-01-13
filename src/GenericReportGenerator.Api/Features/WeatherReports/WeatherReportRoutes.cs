using GenericReportGenerator.Api.Features.WeatherReports.CreateReport;
using GenericReportGenerator.Api.Features.WeatherReports.GetReportFile;
using GenericReportGenerator.Api.Features.WeatherReports.GetReportStatus;

namespace GenericReportGenerator.Api.Features.WeatherReports;

internal static class WeatherReportRoutes
{
    internal static void Map(IEndpointRouteBuilder builder)
    {
        RouteGroupBuilder group = builder
            .MapGroup("weather-report")
            .WithTags("Weather Report");

        CreateReportEndpoint.Map(group);
        GetReportFileEndpoint.Map(group);
        GetReportStatusEndpoint.Map(group);
    }
}
