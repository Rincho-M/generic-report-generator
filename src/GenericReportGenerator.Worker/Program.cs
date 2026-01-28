using GenericReportGenerator.Shared;
using GenericReportGenerator.Worker;
using Serilog;

// Bootstrap logger.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

    IServiceCollection services = builder.Services;

    services.AddSerilog((services, loggerConfig) => loggerConfig.ReadFrom.Configuration(builder.Configuration));

    services.AddServices(builder.Configuration);
    services.AddRepositories(builder.Configuration);
    services.AddIntegrations(builder.Configuration);
    services.AddDatabase(builder.Configuration);
    services.AddMessegeBus(builder.Configuration);
    services.AddCache(builder.Configuration);
    services.AddOptions(builder.Configuration);
    services.AddTelemetry(builder.Configuration);

    IHost host = builder.Build();

    if (args.Contains(TestModeArg))
    {
        TestHost = host;
    }
    else
    {
        host.Run();
    }
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}

/// <summary>
/// For integration testing. Because generic host doesn't have it's own version of WebApplicationFactory.
/// </summary>
public partial class Program
{
    /// <summary>
    /// Provides access to the built host when running in test mode.
    /// </summary>
    public static IHost? TestHost { get; private set; }

    /// <summary>
    /// Argument to run the worker in test mode.
    /// </summary>
    public const string TestModeArg = "--test-mode";
}