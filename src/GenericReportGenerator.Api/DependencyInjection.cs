using GenericReportGenerator.Api.Endpoints.WeatherReport;

namespace GenericReportGenerator.Api;

internal static class DependencyInjection
{
    internal static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder builder)
    {
        RouteGroupBuilder group = builder.MapGroup("api");
        group.MapWeatherReportEndpoints();

        return builder;
    }
}
