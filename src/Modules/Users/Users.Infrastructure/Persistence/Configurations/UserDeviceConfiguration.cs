using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.Domain.Entities;

namespace Users.Infrastructure.Persistence.Configurations;

public class UserDeviceConfiguration : IEntityTypeConfiguration<UserDevice>
{
    public void Configure(EntityTypeBuilder<UserDevice> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.DeviceId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(d => d.DeviceName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(d => d.DeviceType)
            .HasMaxLength(50);

        builder.Property(d => d.IpAddress)
            .HasMaxLength(45);

        builder.Property(d => d.RefreshToken)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(d => d.RevokeReason)
            .HasMaxLength(500);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(d => d.UserId);
        builder.HasIndex(d => d.DeviceId).IsUnique();
        builder.HasIndex(d => d.RefreshToken).IsUnique();
        builder.HasIndex(d => new { d.UserId, d.IsRevoked });
    }
}
