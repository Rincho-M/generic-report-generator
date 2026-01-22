using GenericReportGenerator.Infrastructure.WeatherReports;
using Microsoft.EntityFrameworkCore;

namespace GenericReportGenerator.Infrastructure;

public class AppDbContext : DbContext
{
    public DbSet<Report> WeatherReports { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
