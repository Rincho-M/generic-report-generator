using GenericReportGenerator.Core.Features.WeatherReports.CreateReport;
using GenericReportGenerator.Infrastructure.Features.WeatherReports.ReportFiles;
using GenericReportGenerator.Infrastructure.Features.WeatherReports.WeatherData;
using GenericReportGenerator.Shared;
using MassTransit.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using OpenTelemetry.Trace;

namespace GenericReportGenerator.Worker;

/// <summary>
/// Helper DI methods to use in Program.cs.
/// </summary>
public static class DependencyInjection
{
    public static void AddServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<AddFileService>();
        services.AddScoped<ReportFileBuilder>();
    }

    public static void AddRepositories(this IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<IReportFileRepository, ReportFileRepository>();
    }

    public static void AddIntegrations(this IServiceCollection services, IConfiguration config)
    {
        services
            .AddHttpClient<IWeatherDataRepository, OpenMeteoRepository>()
            .AddStandardResilienceHandler();
        services.AddScoped<OpenMeteoRepository>();
        services.AddScoped<IWeatherDataRepository>(provider =>
        {
            OpenMeteoRepository innerRepository = provider.GetRequiredService<OpenMeteoRepository>();
            IDistributedCache cache = provider.GetRequiredService<IDistributedCache>();
            ILogger<CachedWeatherDataRepository> logger = provider.GetRequiredService<ILogger<CachedWeatherDataRepository>>();
            return new CachedWeatherDataRepository(innerRepository, cache, logger);
        });
    }

    public static void AddOptions(this IServiceCollection services, IConfiguration config)
    {
        IConfigurationSection openMeteoSection = config.GetSection(OpenMeteoOptions.SectionName);
        services.Configure<OpenMeteoOptions>(openMeteoSection);
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
                    .AddSource(DiagnosticHeaders.DefaultListenerName);
            });
    }

    public static void AddCache(this IServiceCollection services, IConfiguration config)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = config.GetRequiredValue("ConnectionStrings:Redis");
        });
    }
}
