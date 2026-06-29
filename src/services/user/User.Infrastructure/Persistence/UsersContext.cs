using Microsoft.EntityFrameworkCore;
using User.Domain.Entities;
using User.Infrastructure.Persistence.Configurations;

namespace User.Infrastructure.Persistence;

public class UsersContext : DbContext
{
    public UsersContext(DbContextOptions<UsersContext> options) : base(options) { }

    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ProfileConfiguration());
        modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
    }
}
