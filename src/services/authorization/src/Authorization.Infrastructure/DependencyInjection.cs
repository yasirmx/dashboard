using Authorization.Application.Abstractions;
using Authorization.Domain.Repositories;
using Authorization.Infrastructure.Persistence;
using Authorization.Infrastructure.Persistence.Repositories;
using Authorization.Infrastructure.Services;
using Authorization.Infrastructure.Settings;
using Authorization.Infrastructure.Workers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Authorization.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Settings
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.Configure<QueueSettings>(configuration.GetSection("Queue"));

        // EF Core
        services.AddDbContext<AuthorizationContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("Authorization"),
                sql => sql.MigrationsAssembly(typeof(AuthorizationContext).Assembly.FullName)));

        // Repositories
        services.AddScoped<ICredentialRepository, CredentialRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();

        // Services
        services.AddSingleton<ITokenService, TokenService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IQueuePublisher, AzureQueuePublisher>();
        services.AddSingleton<IEmailSender, LoggingEmailSender>();

        // Background worker
        services.AddHostedService<RolesChangedConsumer>();

        return services;
    }
}
