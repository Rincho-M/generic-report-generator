using FluentValidation;
using GenericReportGenerator.Api;
using GenericReportGenerator.Api.Features.WeatherReports.CreateReport;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Services.
IServiceCollection services = builder.Services;
services.AddCore();
services.AddInfrastructure(builder.Configuration);
services.AddOpenApi();
services.AddValidatorsFromAssemblyContaining(
    typeof(CreateReportRequestValidator), 
    includeInternalTypes: true);

WebApplication app = builder.Build();

// Middlewares.
app.UseHttpsRedirection();

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
