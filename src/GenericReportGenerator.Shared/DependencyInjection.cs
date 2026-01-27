using GenericReportGenerator.Infrastructure;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GenericReportGenerator.Shared;

/// <summary>
/// Helper DI methods to use in Program.cs.
/// </summary>
public static class DependencyInjection
{
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
                IConfigurationSection rabbitConfig = config.GetRequiredSection("RabbitMq");

                string host = rabbitConfig.GetRequiredValue("Host");
                ushort port = ushort.Parse(rabbitConfig.GetRequiredValue("Port"));
                string virtualHost = rabbitConfig.GetRequiredValue("VirtualHost");
                string password = rabbitConfig.GetRequiredValue("Password");
                string username = rabbitConfig.GetRequiredValue("Username");

                builder.Host(host, port, virtualHost, hostConfig =>
                {
                    hostConfig.Password(password);
                    hostConfig.Username(username);
                });

                builder.UseMessageRetry(r =>
                {
                    IConfigurationSection retryConfig = rabbitConfig.GetRequiredSection("RetryPolicies");

                    int retryCount = int.Parse(retryConfig.GetRequiredValue("Exponential:RetryCount"));
                    TimeSpan minInterval = TimeSpan.Parse(retryConfig.GetRequiredValue("Exponential:MinInterval"));
                    TimeSpan maxInterval = TimeSpan.Parse(retryConfig.GetRequiredValue("Exponential:MaxInterval"));
                    TimeSpan intervalDelta = TimeSpan.Parse(retryConfig.GetRequiredValue("Exponential:IntervalDelta"));

                    r.Exponential(retryCount, minInterval, maxInterval, intervalDelta);
                });

                builder.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
