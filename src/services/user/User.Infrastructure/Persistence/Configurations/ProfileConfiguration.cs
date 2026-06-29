using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using User.Domain.Entities;
using User.Infrastructure.Persistence.Seed;

namespace User.Infrastructure.Persistence.Configurations;

public class ProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> builder)
    {
        builder.ToTable("Profiles");
        builder.HasKey(p => p.UserId);

        builder.Property(p => p.UserId).ValueGeneratedNever();

        builder.Property(p => p.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(p => p.Email).IsUnique();

        builder.Property(p => p.FirstName)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(p => p.LastName)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(p => p.AvatarUrl).HasMaxLength(512);
        builder.Property(p => p.Department).HasMaxLength(128);
        builder.Property(p => p.IsActive).IsRequired();
        builder.Property(p => p.CreatedUtc).IsRequired();

        builder.HasData(SeedData.Users.Select(u => new
        {
            u.UserId,
            Email = u.Email,
            u.FirstName,
            u.LastName,
            AvatarUrl = (string?)$"https://i.pravatar.cc/150?u={u.Email}",
            Department = (string?)u.Department,
            IsActive = true,
            CreatedUtc = SeedData.SeededUtc
        }));
    }
}
