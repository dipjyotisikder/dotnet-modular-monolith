using Bookings.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookings.Infrastructure.Persistence.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.GuestId)
            .IsRequired();

        builder.Property(b => b.HotelId)
            .IsRequired();

        builder.Property(b => b.RoomId)
            .IsRequired();

        builder.Property(b => b.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(b => b.CreatedAt)
            .IsRequired();

        builder.Property(b => b.CancellationReason)
            .HasMaxLength(500);

        builder.OwnsOne(b => b.DateRange, dr =>
        {
            dr.Property(x => x.CheckIn)
                .HasColumnName("CheckInDate")
                .IsRequired();

            dr.Property(x => x.CheckOut)
                .HasColumnName("CheckOutDate")
                .IsRequired();
        });

        builder.OwnsOne(b => b.TotalAmount, m =>
        {
            m.Property(x => x.Amount)
                .HasColumnName("TotalAmount")
                .IsRequired()
                .HasPrecision(18, 2);

            m.Property(x => x.Currency)
                .HasColumnName("TotalAmountCurrency")
                .IsRequired()
                .HasMaxLength(3);
        });

        builder.HasIndex(b => b.GuestId);
        builder.HasIndex(b => b.RoomId);
        builder.HasIndex(b => b.Status);

        builder.Ignore(b => b.DomainEvents);
    }
}
