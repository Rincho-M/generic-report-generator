using GenericReportGenerator.Api.Features.WeatherReports;
using GenericReportGenerator.Core.WeatherReports.GetFile;
using GenericReportGenerator.Core.WeatherReports.GetReport;
using GenericReportGenerator.Core.WeatherReports.QueueReport;
using GenericReportGenerator.Infrastructure;
using GenericReportGenerator.Infrastructure.WeatherReports.ReportFiles;
using MassTransit;
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

    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(config["ConnectionStrings:Database"]);
        });

        return services;
    }

    public static IServiceCollection AddMessegeBus(this IServiceCollection services, IConfiguration config)
    {
        services.AddMassTransit(busBuilder =>
        {
            busBuilder.UsingRabbitMq((context, builder) =>
            {
                // TODO: config validation or something
                string host = config["RabbitMq:Host"];
                ushort port = ushort.Parse(config["RabbitMq:Port"]);
                string virtualHost = config["RabbitMq:VirtualHost"];
                string password = config["RabbitMq:Password"];
                string username = config["RabbitMq:Username"];

                builder.Host(host, port, virtualHost, hostConfig =>
                {
                    hostConfig.Password(password);
                    hostConfig.Username(username);
                });

                builder.UseMessageRetry(r =>
                {
                    // TODO: config validation or something
                    int retryCount = int.Parse(config["RabbitMq:RetryPolicies:Exponential:RetryCount"]);
                    TimeSpan minInterval = TimeSpan.Parse(config["RabbitMq:RetryPolicies:Exponential:MinInterval"]);
                    TimeSpan maxInterval = TimeSpan.Parse(config["RabbitMq:RetryPolicies:Exponential:MaxInterval"]);
                    TimeSpan intervalDelta = TimeSpan.Parse(config["RabbitMq:RetryPolicies:Exponential:IntervalDelta"]);

                    r.Exponential(retryCount, minInterval, maxInterval, intervalDelta);
                });

                builder.ConfigureEndpoints(context);
            });
        });

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
                string[] origins = config.GetSection("Cors:AllowedOrigins").Get<string[]>();
                corsPolicy
                    .WithOrigins(origins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        return services;
    }

    public static IServiceCollection AddForwardedHeaders(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });

        return services;
    }
}
