using Tasks.Domain.Repositories;
using Tasks.Infrastructure.Persistence;
using Tasks.Infrastructure.Persistence.Repositories;
using Tasks.Infrastructure.Services;
using Tasks.Infrastructure.Settings;
using Tasks.Infrastructure.Workers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Tasks.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Settings
        services.Configure<QueueSettings>(configuration.GetSection("Queue"));

        // EF Core
        services.AddDbContext<TaskContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("Tasks"),
                sql => sql.MigrationsAssembly(typeof(TaskContext).Assembly.FullName)));

        // Repositories
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();

        // Services
        services.AddSingleton<AzureQueuePublisher>();

        // Background workers
        services.AddHostedService<OutboxPublisher>();
        services.AddHostedService<SprintEventConsumer>();

        return services;
    }
}
