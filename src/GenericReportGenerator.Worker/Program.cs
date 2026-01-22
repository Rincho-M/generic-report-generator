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
    services.AddOptions(builder.Configuration);
    services.AddTelemetry(builder.Configuration);

    IHost host = builder.Build();

    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}