using GenericReportGenerator.Worker;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

IServiceCollection services = builder.Services;
services.AddCore();
services.AddInfrastructure(builder.Configuration);

IHost host = builder.Build();
host.Run();
