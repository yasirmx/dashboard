using Tasks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Tasks.Infrastructure.Persistence.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Type).IsRequired().HasMaxLength(200);
        builder.Property(o => o.Payload).IsRequired();
        builder.Property(o => o.OccurredUtc).IsRequired();

        builder.HasIndex(o => o.ProcessedUtc);
    }
}
