using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using User.Application.Abstractions;
using User.Domain.Repositories;
using User.Infrastructure.Persistence;
using User.Infrastructure.Persistence.Repositories;
using User.Infrastructure.Services;
using User.Infrastructure.Settings;
using User.Infrastructure.Workers;

namespace User.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Settings
        services.Configure<QueueSettings>(configuration.GetSection("Queue"));

        // EF Core
        services.AddDbContext<UsersContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("Users"),
                sql => sql.MigrationsAssembly(typeof(UsersContext).Assembly.FullName)));

        // Repositories
        services.AddScoped<IProfileRepository, ProfileRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();

        // Services
        services.AddSingleton<IQueuePublisher, AzureQueuePublisher>();

        // Background worker
        services.AddHostedService<UserRegisteredConsumer>();

        return services;
    }
}
