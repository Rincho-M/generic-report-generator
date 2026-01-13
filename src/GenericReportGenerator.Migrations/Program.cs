using GenericReportGenerator.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Configuration.
HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<ApiDbContext>(options =>
{
    string? connectionString = builder.Configuration.GetConnectionString("Database");
    options.UseNpgsql(connectionString, dbBuilder =>
    {
        // This line and -TargetProject parameter in Add-Migration command, force ef to store migrations in this project.
        dbBuilder.MigrationsAssembly(typeof(Program).Assembly);
    });
});

IHost host = builder.Build();

// Migrations.
using IServiceScope scope = host.Services.CreateScope();
ApiDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
ILogger logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

try
{
    logger.LogInformation("Applying database migrations...");
    logger.LogInformation("If target database is clean, a harmless error about missing table can occur.");

    IEnumerable<string> pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
    int migrationsCount = pendingMigrations.Count();
    
    if (migrationsCount > 0)
    {
        logger.LogInformation($"Found {migrationsCount} pending migrations.");

        await dbContext.Database.MigrateAsync();

        logger.LogInformation("Migrations applied successfully.");
    }
    else
    {
        logger.LogInformation("No pending migrations found. Database is up to date.");
    }
}
catch
{
    logger.LogCritical("Database migration failed.");
    throw;
}
