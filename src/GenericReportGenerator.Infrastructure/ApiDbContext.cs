using GenericReportGenerator.Infrastructure.WeatherReports;
using Microsoft.EntityFrameworkCore;

namespace GenericReportGenerator.Infrastructure;

public class ApiDbContext : DbContext
{
    public DbSet<WeatherReport> WeatherReports { get; set; }

    public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApiDbContext).Assembly);
    }
}
