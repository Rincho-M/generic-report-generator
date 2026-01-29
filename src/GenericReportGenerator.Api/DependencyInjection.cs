using GenericReportGenerator.Api.ExceptionHandling;
using GenericReportGenerator.Api.Features.WeatherReports;
using GenericReportGenerator.Core.Features.WeatherReports.CreateReport;
using GenericReportGenerator.Core.Features.WeatherReports.GetFile;
using GenericReportGenerator.Core.Features.WeatherReports.GetReport;
using GenericReportGenerator.Infrastructure.Features.WeatherReports.ReportFiles;
using GenericReportGenerator.Shared;
using MassTransit.Logging;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Trace;

namespace GenericReportGenerator.Api;

/// <summary>
/// Helper DI methods to use in Program.cs.
/// </summary>
public static class DependencyInjection
{
    public static void MapEndpoints(this IEndpointRouteBuilder builder)
    {
        RouteGroupBuilder group = builder.MapGroup("api");

        group.WithMetadata(new ProducesResponseTypeMetadata(
            StatusCodes.Status400BadRequest,
            typeof(ValidationErrorResponse),
            ["application/json"]));

        group.WithMetadata(new ProducesResponseTypeMetadata(
            StatusCodes.Status422UnprocessableEntity,
            typeof(DetailErrorResponse),
            ["application/json"]));

        group.WithMetadata(new ProducesResponseTypeMetadata(
            StatusCodes.Status500InternalServerError,
            typeof(DetailErrorResponse),
            ["application/json"]));

        Routes.Map(group);
    }

    public static void AddServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<QueueReportService>();
        services.AddScoped<GetReportService>();
        services.AddScoped<GetFileSerivce>();
    }

    public static void AddRepositories(this IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<IReportFileRepository, ReportFileRepository>();
    }

    public static void AddOptions(this IServiceCollection services, IConfiguration config)
    {
        IConfigurationSection reportFilesSection = config.GetSection(ReportFilesOptions.SectionName);
        services.Configure<ReportFilesOptions>(reportFilesSection);
    }

    public static void AddTelemetry(this IServiceCollection services, IConfiguration config)
    {
        services.AddOpenTelemetry()
            .WithTracing(tracingBuilder =>
            {
                tracingBuilder
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddSource(DiagnosticHeaders.DefaultListenerName);
            });
    }

    public static void AddCors(this IServiceCollection services, IConfiguration config)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(corsPolicy =>
            {
                string[] origins = config.GetRequiredArray("Cors:AllowedOrigins");
                corsPolicy
                    .WithOrigins(origins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });
    }

    /// <summary>
    /// For tls termination support in case of using reverse proxy.
    /// </summary>
    public static void AddForwardedHeaders(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });
    }
}
