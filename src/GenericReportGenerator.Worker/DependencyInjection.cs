using GenericReportGenerator.Core.WeatherReports;
using GenericReportGenerator.Infrastructure;
using GenericReportGenerator.Infrastructure.WeatherReports;
using GenericReportGenerator.Worker.WeatherReports;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace GenericReportGenerator.Worker;

internal static class DependencyInjection
{
    internal static IServiceCollection AddCore(this IServiceCollection services)
    {
        services.AddScoped<WeatherReportService>();
        services.AddScoped<WeatherReportFileBuilder>();

        return services;
    }
    internal static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // Add postgres.
        services.AddDbContext<ApiDbContext>(options =>
        {
            options
                .UseNpgsql(config["ConnectionStrings:Database"]);
            // TODO: would be nice to add when EFCore.NamingConventions will be updated to 10.0
            //.UseSnakeCaseNamingConvention();
        });

        // Add rabbitmq.
        services.AddMassTransit(busBuilder =>
        {
            busBuilder.AddConsumer<CreateWeatherReportMessageConsumer>();

            busBuilder.UsingRabbitMq((context, builder) =>
            {
                // TODO: config validation or something
                string host = config["RabbitMq:Host"];
                string virtualHost = config["RabbitMq:VirtualHost"];
                string password = config["RabbitMq:Password"];
                string username = config["RabbitMq:Username"];

                builder.Host(host, virtualHost, hostConfig =>
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

        // Add repositories.
        services.AddScoped<IWeatherReportFileRepository, WeatherReportFileRepository>();

        // Add integrations.
        services
            .AddHttpClient<IWeatherDataRepository, OpenMeteoRepository>()
            .AddStandardResilienceHandler();
        services.AddScoped<IWeatherDataRepository, OpenMeteoRepository>();

        // Add typed configs (options pattern).
        IConfigurationSection openMeteoSection = config.GetSection(OpenMeteoOptions.SectionName);
        services.Configure<OpenMeteoOptions>(openMeteoSection);
        IConfigurationSection reportFilesSection = config.GetSection(ReportFilesOptions.SectionName);
        services.Configure<ReportFilesOptions>(reportFilesSection);

        return services;
    }
}
