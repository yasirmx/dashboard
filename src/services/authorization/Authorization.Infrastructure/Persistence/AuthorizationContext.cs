using Authorization.Domain.Entities;
using Authorization.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Authorization.Infrastructure.Persistence;

public class AuthorizationContext : DbContext
{
    public AuthorizationContext(DbContextOptions<AuthorizationContext> options) : base(options) { }

    public DbSet<Credential> Credentials => Set<Credential>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CredentialConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
        modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
    }
}
