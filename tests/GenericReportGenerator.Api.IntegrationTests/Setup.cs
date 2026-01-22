using GenericReportGenerator.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace GenericReportGenerator.Api.IntegrationTests;

/// <summary>
/// One time setup before any tests run.
/// </summary>
[SetUpFixture]
public class Setup
{
    public static PostgreSqlContainer DbContainer { get; private set; }

    public static RabbitMqContainer RabbitContainer { get; private set; }

    public static IConfiguration ApiConfiguration { get; private set; }

    [OneTimeSetUp]
    public async Task RunBeforeAnyTests()
    {
        ApiConfiguration = LoadApiConfiguration();

        await StartTestContainers();

        await ApplyMigrations();
    }

    [OneTimeTearDown]
    public async Task RunAfterAnyTests()
    {
        await DbContainer.DisposeAsync();
        await RabbitContainer.DisposeAsync();
    }

    private static IConfiguration LoadApiConfiguration()
    {
        IConfiguration apiConfig = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        return apiConfig;
    }

    private async Task ApplyMigrations()
    {
        // Apply migrations.
        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(DbContainer.GetConnectionString(), npgsqlBuilder =>
            {
                npgsqlBuilder.MigrationsAssembly(typeof(Migrations.Program).Assembly);
            })
            .Options;
        using AppDbContext context = new(options);
        await context.Database.MigrateAsync();
    }

    private async Task StartTestContainers()
    {
        DbContainer = new PostgreSqlBuilder("postgres:18.1-alpine").Build();
        RabbitContainer = new RabbitMqBuilder("rabbitmq:4.2-management-alpine")
            .WithPortBinding(ApiConfiguration["RabbitMq:Port"], assignRandomHostPort: true)
            .WithUsername(ApiConfiguration["RabbitMq:Username"])
            .WithPassword(ApiConfiguration["RabbitMq:Password"])
            .Build();

        await Task.WhenAll(DbContainer.StartAsync(), RabbitContainer.StartAsync());
    }
}
