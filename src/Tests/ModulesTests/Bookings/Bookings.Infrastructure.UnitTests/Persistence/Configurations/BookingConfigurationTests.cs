using Bookings.Domain.Entities;
using Bookings.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Bookings.Infrastructure.UnitTests.Persistence.Configurations;

public class BookingConfigurationTests
{
    [Fact]
    public void Configure_WithValidBuilder_ConfiguresPrimaryKey()
    {
        var modelBuilder = new ModelBuilder();
        var entityTypeBuilder = modelBuilder.Entity<Booking>();
        var configuration = new BookingConfiguration();

        configuration.Configure(entityTypeBuilder);
        var model = modelBuilder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(Booking));

        Assert.NotNull(entityType);
        var primaryKey = entityType!.FindPrimaryKey();
        Assert.NotNull(primaryKey);
        Assert.Single(primaryKey!.Properties);
        Assert.Equal("Id", primaryKey.Properties[0].Name);
    }

    [Fact]
    public void Configure_WithValidBuilder_ConfiguresGuestIdAsRequired()
    {
        var modelBuilder = new ModelBuilder();
        var entityTypeBuilder = modelBuilder.Entity<Booking>();
        var configuration = new BookingConfiguration();

        configuration.Configure(entityTypeBuilder);
        var model = modelBuilder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(Booking));

        Assert.NotNull(entityType);
        var guestIdProperty = entityType!.FindProperty("GuestId");
        Assert.NotNull(guestIdProperty);
        Assert.False(guestIdProperty!.IsNullable);
    }

    [Fact]
    public void Configure_WithValidBuilder_ConfiguresHotelIdAsRequired()
    {
        var modelBuilder = new ModelBuilder();
        var entityTypeBuilder = modelBuilder.Entity<Booking>();
        var configuration = new BookingConfiguration();

        configuration.Configure(entityTypeBuilder);
        var model = modelBuilder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(Booking));

        Assert.NotNull(entityType);
        var hotelIdProperty = entityType!.FindProperty("HotelId");
        Assert.NotNull(hotelIdProperty);
        Assert.False(hotelIdProperty!.IsNullable);
    }

    [Fact]
    public void Configure_WithValidBuilder_ConfiguresRoomIdAsRequired()
    {
        var modelBuilder = new ModelBuilder();
        var entityTypeBuilder = modelBuilder.Entity<Booking>();
        var configuration = new BookingConfiguration();

        configuration.Configure(entityTypeBuilder);
        var model = modelBuilder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(Booking));

        Assert.NotNull(entityType);
        var roomIdProperty = entityType!.FindProperty("RoomId");
        Assert.NotNull(roomIdProperty);
        Assert.False(roomIdProperty!.IsNullable);
    }

    [Fact]
    public void Configure_WithValidBuilder_ConfiguresStatusWithStringConversionAndMaxLength()
    {
        var modelBuilder = new ModelBuilder();
        var entityTypeBuilder = modelBuilder.Entity<Booking>();
        var configuration = new BookingConfiguration();

        configuration.Configure(entityTypeBuilder);
        var model = modelBuilder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(Booking));

        Assert.NotNull(entityType);
        var statusProperty = entityType!.FindProperty("Status");
        Assert.NotNull(statusProperty);
        Assert.False(statusProperty!.IsNullable);
        Assert.Equal(typeof(string), statusProperty.GetProviderClrType());
        Assert.Equal(50, statusProperty.GetMaxLength());
    }

    [Fact]
    public void Configure_WithValidBuilder_ConfiguresCreatedAtAsRequired()
    {
        var modelBuilder = new ModelBuilder();
        var entityTypeBuilder = modelBuilder.Entity<Booking>();
        var configuration = new BookingConfiguration();

        configuration.Configure(entityTypeBuilder);
        var model = modelBuilder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(Booking));

        Assert.NotNull(entityType);
        var createdAtProperty = entityType!.FindProperty("CreatedAt");
        Assert.NotNull(createdAtProperty);
        Assert.False(createdAtProperty!.IsNullable);
    }

    [Fact]
    public void Configure_WithValidBuilder_ConfiguresCancellationReasonWithMaxLength()
    {
        var modelBuilder = new ModelBuilder();
        var entityTypeBuilder = modelBuilder.Entity<Booking>();
        var configuration = new BookingConfiguration();

        configuration.Configure(entityTypeBuilder);
        var model = modelBuilder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(Booking));

        Assert.NotNull(entityType);
        var cancellationReasonProperty = entityType!.FindProperty("CancellationReason");
        Assert.NotNull(cancellationReasonProperty);
        Assert.Equal(500, cancellationReasonProperty!.GetMaxLength());
    }

    [Fact]
    public void Configure_WithValidBuilder_ConfiguresDateRangeAsOwnedEntity()
    {
        var modelBuilder = new ModelBuilder();
        var entityTypeBuilder = modelBuilder.Entity<Booking>();
        var configuration = new BookingConfiguration();

        configuration.Configure(entityTypeBuilder);
        var model = modelBuilder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(Booking));

        Assert.NotNull(entityType);
        var dateRangeNavigation = entityType!.FindNavigation("DateRange");
        Assert.NotNull(dateRangeNavigation);

        var dateRangeType = dateRangeNavigation!.TargetEntityType;
        var checkInProperty = dateRangeType.FindProperty("CheckIn");
        var checkOutProperty = dateRangeType.FindProperty("CheckOut");

        Assert.NotNull(checkInProperty);
        Assert.NotNull(checkOutProperty);
        Assert.Equal("CheckInDate", checkInProperty!.GetColumnName());
        Assert.Equal("CheckOutDate", checkOutProperty!.GetColumnName());
        Assert.False(checkInProperty.IsNullable);
        Assert.False(checkOutProperty.IsNullable);
    }

    [Fact]
    public void Configure_WithValidBuilder_ConfiguresTotalAmountAsOwnedEntity()
    {
        var modelBuilder = new ModelBuilder();
        var entityTypeBuilder = modelBuilder.Entity<Booking>();
        var configuration = new BookingConfiguration();

        configuration.Configure(entityTypeBuilder);
        var model = modelBuilder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(Booking));

        Assert.NotNull(entityType);
        var ownedNavigation = entityType!.FindNavigation("TotalAmount");
        Assert.NotNull(ownedNavigation);

        var ownedEntityType = ownedNavigation!.TargetEntityType;
        var amountProperty = ownedEntityType.FindProperty("Amount");
        var currencyProperty = ownedEntityType.FindProperty("Currency");

        Assert.NotNull(amountProperty);
        Assert.NotNull(currencyProperty);
        Assert.Equal("TotalAmount", amountProperty!.GetColumnName());
        Assert.Equal("TotalAmountCurrency", currencyProperty!.GetColumnName());
        Assert.False(amountProperty.IsNullable);
        Assert.False(currencyProperty.IsNullable);
        Assert.Equal(18, amountProperty.GetPrecision());
        Assert.Equal(2, amountProperty.GetScale());
        Assert.Equal(3, currencyProperty.GetMaxLength());
    }

    [Fact]
    public void Configure_WithValidBuilder_CreatesIndexOnGuestId()
    {
        var modelBuilder = new ModelBuilder();
        var entityTypeBuilder = modelBuilder.Entity<Booking>();
        var configuration = new BookingConfiguration();

        configuration.Configure(entityTypeBuilder);
        var model = modelBuilder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(Booking));

        Assert.NotNull(entityType);
        var indexes = entityType!.GetIndexes();
        var guestIdIndex = indexes.FirstOrDefault(i =>
            i.Properties.Count == 1 && i.Properties[0].Name == "GuestId");
        Assert.NotNull(guestIdIndex);
    }

    [Fact]
    public void Configure_WithValidBuilder_CreatesIndexOnRoomId()
    {
        var modelBuilder = new ModelBuilder();
        var entityTypeBuilder = modelBuilder.Entity<Booking>();
        var configuration = new BookingConfiguration();

        configuration.Configure(entityTypeBuilder);
        var model = modelBuilder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(Booking));

        Assert.NotNull(entityType);
        var indexes = entityType!.GetIndexes();
        var roomIdIndex = indexes.FirstOrDefault(i =>
            i.Properties.Count == 1 && i.Properties[0].Name == "RoomId");
        Assert.NotNull(roomIdIndex);
    }

    [Fact]
    public void Configure_WithValidBuilder_CreatesIndexOnStatus()
    {
        var modelBuilder = new ModelBuilder();
        var entityTypeBuilder = modelBuilder.Entity<Booking>();
        var configuration = new BookingConfiguration();

        configuration.Configure(entityTypeBuilder);
        var model = modelBuilder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(Booking));

        Assert.NotNull(entityType);
        var indexes = entityType!.GetIndexes();
        var statusIndex = indexes.FirstOrDefault(i =>
            i.Properties.Count == 1 && i.Properties[0].Name == "Status");
        Assert.NotNull(statusIndex);
    }

    [Fact]
    public void Configure_WithValidBuilder_IgnoresDomainEventsProperty()
    {
        var modelBuilder = new ModelBuilder();
        var entityTypeBuilder = modelBuilder.Entity<Booking>();
        var configuration = new BookingConfiguration();

        configuration.Configure(entityTypeBuilder);
        var model = modelBuilder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(Booking));

        Assert.NotNull(entityType);
        var domainEventsProperty = entityType!.FindProperty("DomainEvents");
        Assert.Null(domainEventsProperty);
    }

    [Fact]
    public void Configure_WithValidBuilder_CreatesExactlyThreeIndexes()
    {
        var modelBuilder = new ModelBuilder();
        var entityTypeBuilder = modelBuilder.Entity<Booking>();
        var configuration = new BookingConfiguration();

        configuration.Configure(entityTypeBuilder);
        var model = modelBuilder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(Booking));

        Assert.NotNull(entityType);
        var indexes = entityType!.GetIndexes().ToList();
        Assert.Equal(3, indexes.Count);
    }

    [Fact]
    public void Configure_WithValidBuilder_DoesNotThrowException()
    {
        var modelBuilder = new ModelBuilder();
        var entityTypeBuilder = modelBuilder.Entity<Booking>();
        var configuration = new BookingConfiguration();

        var exception = Record.Exception(() => configuration.Configure(entityTypeBuilder));
        Assert.Null(exception);
    }
}
