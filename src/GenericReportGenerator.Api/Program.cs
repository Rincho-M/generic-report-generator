using GenericReportGenerator.Api;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Services.
builder.Services.AddOpenApi();

// Middlewares.
var app = builder.Build();

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
