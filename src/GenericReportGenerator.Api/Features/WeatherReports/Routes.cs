using GenericReportGenerator.Api.Features.WeatherReports.CreateReport;
using GenericReportGenerator.Api.Features.WeatherReports.GetReport;
using GenericReportGenerator.Api.Features.WeatherReports.GetReportFile;

namespace GenericReportGenerator.Api.Features.WeatherReports;

/// <summary>
/// Setup routing for weather report endpoints.
/// </summary>
public static class Routes
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        RouteGroupBuilder group = builder
            .MapGroup("weather-report")
            .WithTags("Weather Report");

        CreateReportEndpoint.Map(group);
        GetReportFileEndpoint.Map(group);
        GetReportEndpoint.Map(group);
    }
}
