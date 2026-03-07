using Bookings.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookings.Infrastructure.Persistence.Configurations;

public class HotelConfiguration : IEntityTypeConfiguration<Hotel>
{
    public void Configure(EntityTypeBuilder<Hotel> builder)
    {
        builder.HasKey(h => h.Id);

        builder.Property(h => h.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(h => h.Description)
            .HasMaxLength(1000);

        builder.Property(h => h.StarRating)
            .IsRequired();

        builder.Property(h => h.IsActive)
            .IsRequired();

        builder.Property(h => h.CreatedAt)
            .IsRequired();

        builder.OwnsOne(h => h.Address, a =>
        {
            a.Property(x => x.Street).HasColumnName("Street").IsRequired().HasMaxLength(300);
            a.Property(x => x.City).HasColumnName("City").IsRequired().HasMaxLength(100);
            a.Property(x => x.Country).HasColumnName("Country").IsRequired().HasMaxLength(100);
            a.Property(x => x.ZipCode).HasColumnName("ZipCode").HasMaxLength(20);
        });

        builder.HasMany(h => h.Rooms)
            .WithOne()
            .HasForeignKey(r => r.HotelId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(h => h.Rooms).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(h => h.DomainEvents);
    }
}
