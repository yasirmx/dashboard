using Tasks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Tasks.Infrastructure.Persistence.Configurations;

public class TaskActivityConfiguration : IEntityTypeConfiguration<TaskActivity>
{
    public void Configure(EntityTypeBuilder<TaskActivity> builder)
    {
        builder.ToTable("TaskActivities");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Type).IsRequired().HasMaxLength(100);
        builder.Property(a => a.OldValue).HasMaxLength(500);
        builder.Property(a => a.NewValue).HasMaxLength(500);
        builder.Property(a => a.CreatedUtc).IsRequired();

        builder.HasIndex(a => a.TaskId);
    }
}
