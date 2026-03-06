using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.Domain.Entities;

namespace Users.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.PasswordHash)
            .HasMaxLength(500);

        builder.Property(u => u.Tier)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.Roles)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(u => u.OAuthProvider)
            .HasMaxLength(50);

        builder.Property(u => u.OAuthProviderId)
            .HasMaxLength(200);

        builder.HasIndex(u => new { u.OAuthProvider, u.OAuthProviderId });

        builder.Property(u => u.RefreshToken)
            .HasMaxLength(500);

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Ignore(u => u.DomainEvents);
    }
}
