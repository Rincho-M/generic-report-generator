using GenericReportGenerator.Core.WeatherReports.AddFile;
using GenericReportGenerator.Infrastructure.WeatherReports.ReportFiles;
using GenericReportGenerator.Infrastructure.WeatherReports.WeatherData;
using MassTransit.Logging;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Trace;

namespace GenericReportGenerator.Worker;

/// <summary>
/// Helper DI methods to use in Program.cs.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<AddFileService>();
        services.AddScoped<ReportFileBuilder>();

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<IReportFileRepository, ReportFileRepository>();

        return services;
    }

    public static IServiceCollection AddIntegrations(this IServiceCollection services, IConfiguration config)
    {
        services
            .AddHttpClient<IWeatherDataRepository, OpenMeteoRepository>()
            .AddStandardResilienceHandler();
        services.AddScoped<IWeatherDataRepository, OpenMeteoRepository>();

        return services;
    }

    public static IServiceCollection AddOptions(this IServiceCollection services, IConfiguration config)
    {
        IConfigurationSection openMeteoSection = config.GetSection(OpenMeteoOptions.SectionName);
        services.Configure<OpenMeteoOptions>(openMeteoSection);
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
                    .AddSource(DiagnosticHeaders.DefaultListenerName);
            });

        return services;
    }
}
