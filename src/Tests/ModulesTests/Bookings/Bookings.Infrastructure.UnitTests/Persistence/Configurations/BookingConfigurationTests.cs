namespace Bookings.Infrastructure.UnitTests.Persistence.Configurations;

/// <summary>
/// Unit tests for the <see cref="BookingConfiguration"/> class.
/// </summary>
public class BookingConfigurationTests
{
    /// <summary>
    /// Tests that the Configure method successfully configures the Booking entity
    /// and sets up the primary key correctly.
    /// </summary>
    [Fact]
    public void Configure_WithValidBuilder_ConfiguresPrimaryKey()
    {
        // Arrange
        var modelBuilder = new ModelBuilder();
        var entityTypeBuilder = modelBuilder.Entity<Booking>();
        var configuration = new BookingConfiguration();

        // Act
        configuration.Configure(entityTypeBuilder);
        var model = modelBuilder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(Booking));

        // Assert
        Assert.NotNull(entityType);
        var primaryKey = entityType!.FindPrimaryKey();
        Assert.NotNull(primaryKey);
        Assert.Single(primaryKey!.Properties);
        Assert.Equal("Id", primaryKey.Properties[0].Name);
    }

    /// <summary>
    /// Tests that the Configure method marks GuestId as required.
    /// </summary>
    [Fact]
    public void Configure_WithValidBuilder_ConfiguresGuestIdAsRequired()
    {
        // Arrange
        var modelBuilder = new ModelBuilder();
        var entityTypeBuilder = modelBuilder.Entity<Booking>();
        var configuration = new BookingConfiguration();

        // Act
        configuration.Configure(entityTypeBuilder);
        var model = modelBuilder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(Booking));

        // Assert
        Assert.NotNull(entityType);
        var guestIdProperty = entityType!.FindProperty("GuestId");
        Assert.NotNull(guestIdProperty);
        Assert.False(guestIdProperty!.IsNullable);
    }

    /// <summary>
    /// Tests that the Configure method marks HotelId as required.
    /// </summary>
    [Fact]
    public void Configure_WithValidBuilder_ConfiguresHotelIdAsRequired()
    {
        // Arrange
        var modelBuilder = new ModelBuilder();
        var entityTypeBuilder = modelBuilder.Entity<Booking>();
        var configuration = new BookingConfiguration();

        // Act
        configuration.Configure(entityTypeBuilder);
        var model = modelBuilder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(Booking));

        // Assert
        Assert.NotNull(entityType);
        var hotelIdProperty = entityType!.FindProperty("HotelId");
        Assert.NotNull(hotelIdProperty);
        Assert.False(hotelIdProperty!.IsNullable);
    }

    /// <summary>
    /// Tests that the Configure method marks RoomId as required.
    /// </summary>
    [Fact]
    public void Configure_WithValidBuilder_ConfiguresRoomIdAsRequired()
    {
        // Arrange
        var modelBuilder = new ModelBuilder();
        var entityTypeBuilder = modelBuilder.Entity<Booking>();
        var configuration = new BookingConfiguration();

        // Act
        configuration.Configure(entityTypeBuilder);
        var model = modelBuilder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(Booking));

        // Assert
        Assert.NotNull(entityType);
        var roomIdProperty = entityType!.FindProperty("RoomId");
        Assert.NotNull(roomIdProperty);
        Assert.False(roomIdProperty!.IsNullable);
    }

    /// <summary>
    /// Tests that the Configure method configures Status as required with string conversion
    /// and a maximum length of 50.
    /// </summary>
    [Fact]
    public void Configure_WithValidBuilder_ConfiguresStatusWithStringConversionAndMaxLength()
    {
        // Arrange
        var modelBuilder = new ModelBuilder();
        var entityTypeBuilder = modelBuilder.Entity<Booking>();
        var configuration = new BookingConfiguration();

        // Act
        configuration.Configure(entityTypeBuilder);
        var model = modelBuilder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(Booking));

        // Assert
        Assert.NotNull(entityType);
        var statusProperty = entityType!.FindProperty("Status");
        Assert.NotNull(statusProperty);
        Assert.False(statusProperty!.IsNullable);
        Assert.Equal(typeof(string), statusProperty.GetProviderClrType());
        Assert.Equal(50, statusProperty.GetMaxLength());
    }

    /// <summary>
    /// Tests that the Configure method marks CreatedAt as required.
    /// </summary>
    [Fact]
    public void Configure_WithValidBuilder_ConfiguresCreatedAtAsRequired()
    {
        // Arrange
        var modelBuilder = new ModelBuilder();
        var entityTypeBuilder = modelBuilder.Entity<Booking>();
        var configuration = new BookingConfiguration();

        // Act
        configuration.Configure(entityTypeBuilder);
        var model = modelBuilder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(Booking));

        // Assert
        Assert.NotNull(entityType);
        var createdAtProperty = entityType!.FindProperty("CreatedAt");
        Assert.NotNull(createdAtProperty);
        Assert.False(createdAtProperty!.IsNullable);
    }

    /// <summary>
    /// Tests that the Configure method configures CancellationReason with a maximum length of 500.
    /// </summary>
    [Fact]
    public void Configure_WithValidBuilder_ConfiguresCancellationReasonWithMaxLength()
    {
        // Arrange
        var modelBuilder = new ModelBuilder();
        var entityTypeBuilder = modelBuilder.Entity<Booking>();
        var configuration = new BookingConfiguration();

        // Act
        configuration.Configure(entityTypeBuilder);
        var model = modelBuilder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(Booking));

        // Assert
        Assert.NotNull(entityType);
        var cancellationReasonProperty = entityType!.FindProperty("CancellationReason");
        Assert.NotNull(cancellationReasonProperty);
        Assert.Equal(500, cancellationReasonProperty!.GetMaxLength());
    }

    /// <summary>
    /// Tests that the Configure method configures DateRange as an owned entity
    /// with CheckIn and CheckOut properties mapped to custom column names.
    /// </summary>
    [Fact]
    public void Configure_WithValidBuilder_ConfiguresDateRangeAsOwnedEntity()
    {
        // Arrange
        var modelBuilder = new ModelBuilder();
        var entityTypeBuilder = modelBuilder.Entity<Booking>();
        var configuration = new BookingConfiguration();

        // Act
        configuration.Configure(entityTypeBuilder);
        var model = modelBuilder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(Booking));

        // Assert
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

    /// <summary>
    /// Tests that the Configure method configures TotalAmount (Money) as an owned entity
    /// with Amount and Currency properties mapped to custom column names with precision.
    /// </summary>
    [Fact]
    public void Configure_WithValidBuilder_ConfiguresTotalAmountAsOwnedEntity()
    {
        // Arrange
        var modelBuilder = new ModelBuilder();
        var entityTypeBuilder = modelBuilder.Entity<Booking>();
        var configuration = new BookingConfiguration();

        // Act
        configuration.Configure(entityTypeBuilder);
        var model = modelBuilder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(Booking));

        // Assert
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

    /// <summary>
    /// Tests that the Configure method creates an index on the GuestId property.
    /// </summary>
    [Fact]
    public void Configure_WithValidBuilder_CreatesIndexOnGuestId()
    {
        // Arrange
        var modelBuilder = new ModelBuilder();
        var entityTypeBuilder = modelBuilder.Entity<Booking>();
        var configuration = new BookingConfiguration();

        // Act
        configuration.Configure(entityTypeBuilder);
        var model = modelBuilder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(Booking));

        // Assert
        Assert.NotNull(entityType);
        var indexes = entityType!.GetIndexes();
        var guestIdIndex = indexes.FirstOrDefault(i =>
            i.Properties.Count == 1 && i.Properties[0].Name == "GuestId");
        Assert.NotNull(guestIdIndex);
    }

    /// <summary>
    /// Tests that the Configure method creates an index on the RoomId property.
    /// </summary>
    [Fact]
    public void Configure_WithValidBuilder_CreatesIndexOnRoomId()
    {
        // Arrange
        var modelBuilder = new ModelBuilder();
        var entityTypeBuilder = modelBuilder.Entity<Booking>();
        var configuration = new BookingConfiguration();

        // Act
        configuration.Configure(entityTypeBuilder);
        var model = modelBuilder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(Booking));

        // Assert
        Assert.NotNull(entityType);
        var indexes = entityType!.GetIndexes();
        var roomIdIndex = indexes.FirstOrDefault(i =>
            i.Properties.Count == 1 && i.Properties[0].Name == "RoomId");
        Assert.NotNull(roomIdIndex);
    }

    /// <summary>
    /// Tests that the Configure method creates an index on the Status property.
    /// </summary>
    [Fact]
    public void Configure_WithValidBuilder_CreatesIndexOnStatus()
    {
        // Arrange
        var modelBuilder = new ModelBuilder();
        var entityTypeBuilder = modelBuilder.Entity<Booking>();
        var configuration = new BookingConfiguration();

        // Act
        configuration.Configure(entityTypeBuilder);
        var model = modelBuilder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(Booking));

        // Assert
        Assert.NotNull(entityType);
        var indexes = entityType!.GetIndexes();
        var statusIndex = indexes.FirstOrDefault(i =>
            i.Properties.Count == 1 && i.Properties[0].Name == "Status");
        Assert.NotNull(statusIndex);
    }

    /// <summary>
    /// Tests that the Configure method ignores the DomainEvents property,
    /// ensuring it is not part of the entity model.
    /// </summary>
    [Fact]
    public void Configure_WithValidBuilder_IgnoresDomainEventsProperty()
    {
        // Arrange
        var modelBuilder = new ModelBuilder();
        var entityTypeBuilder = modelBuilder.Entity<Booking>();
        var configuration = new BookingConfiguration();

        // Act
        configuration.Configure(entityTypeBuilder);
        var model = modelBuilder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(Booking));

        // Assert
        Assert.NotNull(entityType);
        var domainEventsProperty = entityType!.FindProperty("DomainEvents");
        Assert.Null(domainEventsProperty);
    }

    /// <summary>
    /// Tests that the Configure method creates exactly three indexes:
    /// one for GuestId, one for RoomId, and one for Status.
    /// </summary>
    [Fact]
    public void Configure_WithValidBuilder_CreatesExactlyThreeIndexes()
    {
        // Arrange
        var modelBuilder = new ModelBuilder();
        var entityTypeBuilder = modelBuilder.Entity<Booking>();
        var configuration = new BookingConfiguration();

        // Act
        configuration.Configure(entityTypeBuilder);
        var model = modelBuilder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(Booking));

        // Assert
        Assert.NotNull(entityType);
        var indexes = entityType!.GetIndexes().ToList();
        Assert.Equal(3, indexes.Count);
    }

    /// <summary>
    /// Tests that the Configure method does not throw any exceptions
    /// when provided with a valid EntityTypeBuilder.
    /// </summary>
    [Fact]
    public void Configure_WithValidBuilder_DoesNotThrowException()
    {
        // Arrange
        var modelBuilder = new ModelBuilder();
        var entityTypeBuilder = modelBuilder.Entity<Booking>();
        var configuration = new BookingConfiguration();

        // Act & Assert
        var exception = Record.Exception(() => configuration.Configure(entityTypeBuilder));
        Assert.Null(exception);
    }
}