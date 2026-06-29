using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using User.Domain.Entities;
using User.Infrastructure.Persistence.Seed;

namespace User.Infrastructure.Persistence.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles");
        builder.HasKey(r => new { r.UserId, r.Role });

        builder.Property(r => r.Role)
            .IsRequired()
            .HasMaxLength(64);

        builder.HasData(
            SeedData.Users.SelectMany(u => u.Roles.Select(role => new { u.UserId, Role = role })));
    }
}
