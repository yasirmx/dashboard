using Tasks.Domain.Entities;
using Tasks.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Tasks.Infrastructure.Persistence;

public class TaskContext : DbContext
{
    public TaskContext(DbContextOptions<TaskContext> options) : base(options) { }

    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<TaskTag> TaskTags => Set<TaskTag>();
    public DbSet<TaskActivity> TaskActivities => Set<TaskActivity>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new TaskItemConfiguration());
        modelBuilder.ApplyConfiguration(new TaskTagConfiguration());
        modelBuilder.ApplyConfiguration(new TaskActivityConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
    }
}
