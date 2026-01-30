using System.Reflection;
using GenericReportGenerator.Infrastructure.Common;
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
    public static void AddDatabase(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            string dbConnectionString = config.GetRequiredValue("ConnectionStrings:Database");
            options.UseNpgsql(dbConnectionString);
        });
    }

    public static void AddMessegeBus(
        this IServiceCollection services, IConfiguration config)
    {
        Assembly consumersAssembly = Assembly.GetCallingAssembly();

        services.AddMassTransit(busBuilder =>
        {
            busBuilder.AddConsumers(consumersAssembly);

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
    }
}
