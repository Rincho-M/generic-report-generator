using GenericReportGenerator.Api.Features.WeatherReports;
using GenericReportGenerator.Core.WeatherReports.GetFile;
using GenericReportGenerator.Core.WeatherReports.GetReport;
using GenericReportGenerator.Core.WeatherReports.QueueReport;
using GenericReportGenerator.Infrastructure.WeatherReports.ReportFiles;
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
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder builder)
    {
        RouteGroupBuilder group = builder.MapGroup("api");

        Routes.Map(group);

        return builder;
    }

    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<QueueReportService>();
        services.AddScoped<GetReportService>();
        services.AddScoped<GetFileSerivce>();

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<IReportFileRepository, ReportFileRepository>();

        return services;
    }

    public static IServiceCollection AddOptions(this IServiceCollection services, IConfiguration config)
    {
        IConfigurationSection reportFilesSection = config.GetSection(ReportFilesOptions.SectionName);
        services.Configure<ReportFilesOptions>(reportFilesSection);

        return services;
    }

    public static IServiceCollection AddTelemetry(this IServiceCollection services, IConfiguration config)
    {
        services.AddOpenTelemetry()
            .WithTracing(tracingBuilder =>
            {
                tracingBuilder
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddSource(DiagnosticHeaders.DefaultListenerName);
            });

        return services;
    }

    public static IServiceCollection AddCors(this IServiceCollection services, IConfiguration config)
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

        return services;
    }

    /// <summary>
    /// For tls termination support in case of using reverse proxy.
    /// </summary>
    public static IServiceCollection AddForwardedHeaders(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });

        return services;
    }
}
