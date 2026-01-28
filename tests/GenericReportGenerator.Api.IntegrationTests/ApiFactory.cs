using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace GenericReportGenerator.Api.IntegrationTests;

/// <summary>
/// Provides an instance of Api project for integration testing scenarios.
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Inject the TestContainer connection strings to api config.
            Dictionary<string, string?> overrides = new()
            {
                { "ConnectionStrings:Database", Setup.DbContainer.GetConnectionString() },
                { "RabbitMq:Host", Setup.RabbitContainer.Hostname },
                { "RabbitMq:Port", Setup.RabbitContainer.GetMappedPublicPort(Setup.ApiConfiguration["RabbitMq:Port"]).ToString() },
            };

            config.AddInMemoryCollection(overrides);
        });
    }
}
