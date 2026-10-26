using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenericReportGenerator.Infrastructure.Common.Outbox;

/// <summary>
/// Durable outbox row written in the same transaction as the domain change that produced a message.
/// </summary>
public class OutboxMessage
{
    public Guid Id { get; set; }

    public required string Type { get; set; }

    public required string Payload { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? ProcessedAt { get; set; }

    public class EntityConfiguration : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.ToTable("outbox_message");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .IsRequired();

            builder.Property(x => x.Type)
                .IsRequired()
                .HasMaxLength(128);

            builder.Property(x => x.Payload)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.ProcessedAt)
                .IsRequired(false);

            builder.HasIndex(x => x.CreatedAt)
                .HasFilter("\"ProcessedAt\" IS NULL");
        }
    }
}
