using Bookings.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookings.Infrastructure.Persistence.Configurations;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.HotelId)
            .IsRequired();

        builder.Property(r => r.RoomNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(r => r.RoomType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(r => r.MaxOccupancy)
            .IsRequired();

        builder.Property(r => r.Description)
            .HasMaxLength(500);

        builder.Property(r => r.IsAvailable)
            .IsRequired();

        builder.OwnsOne(r => r.PricePerNight, m =>
        {
            m.Property(x => x.Amount)
                .HasColumnName("PricePerNightAmount")
                .IsRequired()
                .HasPrecision(18, 2);

            m.Property(x => x.Currency)
                .HasColumnName("PricePerNightCurrency")
                .IsRequired()
                .HasMaxLength(3);
        });

        builder.HasIndex(r => new { r.HotelId, r.RoomNumber }).IsUnique();

        builder.Ignore(r => r.DomainEvents);
    }
}
