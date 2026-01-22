using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenericReportGenerator.Infrastructure.WeatherReports;

/// <summary>
/// Represents a weather report entity.
/// </summary>
public class Report
{
    public Guid Id { get; set; }

    public ReportStatus Status { get; set; }

    public required string City { get; set; }

    public DateOnly FromDate { get; set; }

    public DateOnly ToDate { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public string? FilePath { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public class EntityConfiguration : IEntityTypeConfiguration<Report>
    {
        public void Configure(EntityTypeBuilder<Report> builder)
        {
            builder.ToTable("weather_report");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .IsRequired();

            builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.City)
                .IsRequired()
                .HasMaxLength(128);

            builder.Property(x => x.FromDate)
                .IsRequired();

            builder.Property(x => x.ToDate)
                .IsRequired();

            builder.Property(x => x.CompletedAt)
                .IsRequired(false);

            builder.Property(x => x.FilePath)
                .IsRequired(false)
                .HasMaxLength(1024);

            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.CreatedAt);
        }
    }
}