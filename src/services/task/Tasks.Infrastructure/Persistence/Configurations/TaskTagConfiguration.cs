using Tasks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Tasks.Infrastructure.Persistence.Configurations;

public class TaskTagConfiguration : IEntityTypeConfiguration<TaskTag>
{
    public void Configure(EntityTypeBuilder<TaskTag> builder)
    {
        builder.ToTable("TaskTags");
        builder.HasKey(t => new { t.TaskId, t.Tag });

        builder.Property(t => t.Tag)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasOne<TaskItem>()
            .WithMany(t => t.Tags)
            .HasForeignKey(t => t.TaskId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
