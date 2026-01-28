using FluentValidation;
using GenericReportGenerator.Api;
using GenericReportGenerator.Api.ExceptionHandling;
using GenericReportGenerator.Shared;
using Serilog;

// Bootstrap logger.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    // Services.
    IServiceCollection services = builder.Services;

    services.AddSerilog((services, logConfig) => logConfig.ReadFrom.Configuration(builder.Configuration));

    services.AddServices(builder.Configuration);
    services.AddRepositories(builder.Configuration);
    services.AddDatabase(builder.Configuration);
    services.AddMessegeBus(builder.Configuration);
    services.AddOptions(builder.Configuration);
    services.AddTelemetry(builder.Configuration);
    services.AddCors(builder.Configuration);
    services.AddForwardedHeaders(builder.Configuration);

    services.AddOpenApi();
    services.AddValidatorsFromAssemblyContaining(typeof(Program), includeInternalTypes: true);
    services.AddExceptionHandler<GlobalExceptionHandler>();

    WebApplication app = builder.Build();

    // Middlewares.
    app.UseHttpsRedirection();
    app.UseExceptionHandler(_ => { });
    app.UseCors();
    app.UseForwardedHeaders();

    // Endpoints.
    app.MapEndpoints();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint(
                url: "/openapi/v1.json",
                name: "Generic Report Generator API v1");
        });
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}

