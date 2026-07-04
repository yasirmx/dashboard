using Tasks.Domain.Entities;
using Tasks.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Tasks.Infrastructure.Persistence.Configurations;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("Tasks");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(t => t.Description)
            .HasMaxLength(4000);

        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(t => t.Priority)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(t => t.CreatedByUserId).IsRequired();
        builder.Property(t => t.OrderIndex).IsRequired();
        builder.Property(t => t.CreatedUtc).IsRequired();
        builder.Property(t => t.UpdatedUtc).IsRequired();

        // Optimistic concurrency
        builder.Property(t => t.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // Self-referencing FK for sub-tasks
        builder.HasOne<TaskItem>()
            .WithMany(t => t.SubTasks)
            .HasForeignKey(t => t.ParentTaskId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(t => t.SprintId);
        builder.HasIndex(t => t.AssigneeId);
        builder.HasIndex(t => t.ParentTaskId);
        builder.HasIndex(t => t.Status);
    }
}
