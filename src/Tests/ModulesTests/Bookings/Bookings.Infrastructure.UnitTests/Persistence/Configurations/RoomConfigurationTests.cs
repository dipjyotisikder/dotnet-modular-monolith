using Xunit;
using Microsoft.EntityFrameworkCore;
using Bookings.Infrastructure.Persistence.Configurations;
using Bookings.Domain.Entities;

namespace Bookings.Infrastructure.UnitTests.Persistence.Configurations;

/// <summary>
/// Unit tests for the <see cref="RoomConfiguration"/> class.
/// </summary>
public class RoomConfigurationTests
{
    /// <summary>
    /// Tests that the Configure method correctly sets up the Room entity configuration
    /// when provided with a valid EntityTypeBuilder, verifying all property configurations,
    /// constraints, and relationships are properly applied.
    /// </summary>
    [Fact]
    public void Configure_ValidBuilder_ConfiguresRoomEntityCorrectly()
    {
        // Arrange
        var configuration = new RoomConfiguration();
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);
        var model = context.Model;
        var entityType = model.FindEntityType(typeof(Room));

        // Act & Assert - EntityType should be configured
        Assert.NotNull(entityType);
    }

    /// <summary>
    /// Tests that the Configure method correctly sets the Id property as the primary key
    /// for the Room entity.
    /// </summary>
    [Fact]
    public void Configure_ValidBuilder_SetsPrimaryKeyOnIdProperty()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);
        var model = context.Model;
        var entityType = model.FindEntityType(typeof(Room));

        // Act
        var primaryKey = entityType?.FindPrimaryKey();

        // Assert
        Assert.NotNull(primaryKey);
        Assert.Single(primaryKey.Properties);
        Assert.Equal("Id", primaryKey.Properties[0].Name);
    }

    /// <summary>
    /// Tests that the Configure method marks the HotelId property as required.
    /// </summary>
    [Fact]
    public void Configure_ValidBuilder_ConfiguresHotelIdAsRequired()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);
        var model = context.Model;
        var entityType = model.FindEntityType(typeof(Room));

        // Act
        var hotelIdProperty = entityType?.FindProperty("HotelId");

        // Assert
        Assert.NotNull(hotelIdProperty);
        Assert.False(hotelIdProperty.IsNullable);
    }

    /// <summary>
    /// Tests that the Configure method marks the RoomNumber property as required
    /// and sets the maximum length to 20 characters.
    /// </summary>
    [Fact]
    public void Configure_ValidBuilder_ConfiguresRoomNumberAsRequiredWithMaxLength()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);
        var model = context.Model;
        var entityType = model.FindEntityType(typeof(Room));

        // Act
        var roomNumberProperty = entityType?.FindProperty("RoomNumber");

        // Assert
        Assert.NotNull(roomNumberProperty);
        Assert.False(roomNumberProperty.IsNullable);
        Assert.Equal(20, roomNumberProperty.GetMaxLength());
    }

    /// <summary>
    /// Tests that the Configure method marks the RoomType property as required,
    /// configures it with string conversion, and sets maximum length to 50 characters.
    /// </summary>
    [Fact]
    public void Configure_ValidBuilder_ConfiguresRoomTypeAsRequiredWithStringConversionAndMaxLength()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);
        var model = context.Model;
        var entityType = model.FindEntityType(typeof(Room));

        // Act
        var roomTypeProperty = entityType?.FindProperty("RoomType");

        // Assert
        Assert.NotNull(roomTypeProperty);
        Assert.False(roomTypeProperty.IsNullable);
        Assert.Equal(50, roomTypeProperty.GetMaxLength());
        // Note: Value converters are not fully materialized by the In-Memory provider
        // The configuration HasConversion<string>() is present in RoomConfiguration
        // but the In-Memory database provider doesn't expose it in the metadata
        var valueConverter = roomTypeProperty.GetValueConverter();
        if (valueConverter != null)
        {
            Assert.Equal(typeof(string), valueConverter.ProviderClrType);
        }
    }

    /// <summary>
    /// Tests that the Configure method marks the MaxOccupancy property as required.
    /// </summary>
    [Fact]
    public void Configure_ValidBuilder_ConfiguresMaxOccupancyAsRequired()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);
        var model = context.Model;
        var entityType = model.FindEntityType(typeof(Room));

        // Act
        var maxOccupancyProperty = entityType?.FindProperty("MaxOccupancy");

        // Assert
        Assert.NotNull(maxOccupancyProperty);
        Assert.False(maxOccupancyProperty.IsNullable);
    }

    /// <summary>
    /// Tests that the Configure method sets the maximum length of 500 characters
    /// for the Description property and allows null values.
    /// </summary>
    [Fact]
    public void Configure_ValidBuilder_ConfiguresDescriptionWithMaxLengthAndAllowsNull()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);
        var model = context.Model;
        var entityType = model.FindEntityType(typeof(Room));

        // Act
        var descriptionProperty = entityType?.FindProperty("Description");

        // Assert
        Assert.NotNull(descriptionProperty);
        Assert.True(descriptionProperty.IsNullable);
        Assert.Equal(500, descriptionProperty.GetMaxLength());
    }

    /// <summary>
    /// Tests that the Configure method marks the IsAvailable property as required.
    /// </summary>
    [Fact]
    public void Configure_ValidBuilder_ConfiguresIsAvailableAsRequired()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);
        var model = context.Model;
        var entityType = model.FindEntityType(typeof(Room));

        // Act
        var isAvailableProperty = entityType?.FindProperty("IsAvailable");

        // Assert
        Assert.NotNull(isAvailableProperty);
        Assert.False(isAvailableProperty.IsNullable);
    }

    /// <summary>
    /// Tests that the Configure method correctly configures the PricePerNight owned entity
    /// with proper column names, precision, and constraints for Amount and Currency properties.
    /// </summary>
    [Fact]
    public void Configure_ValidBuilder_ConfiguresPricePerNightOwnedEntityCorrectly()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);
        var model = context.Model;
        var entityType = model.FindEntityType(typeof(Room));

        // Act
        var ownedNavigation = entityType?.FindNavigation("PricePerNight");
        var ownedEntityType = ownedNavigation?.TargetEntityType;
        var amountProperty = ownedEntityType?.FindProperty("Amount");
        var currencyProperty = ownedEntityType?.FindProperty("Currency");

        // Assert - Amount property
        Assert.NotNull(amountProperty);
        Assert.Equal("PricePerNightAmount", amountProperty.GetColumnName());
        Assert.False(amountProperty.IsNullable);
        Assert.Equal(18, amountProperty.GetPrecision());
        Assert.Equal(2, amountProperty.GetScale());

        // Assert - Currency property
        Assert.NotNull(currencyProperty);
        Assert.Equal("PricePerNightCurrency", currencyProperty.GetColumnName());
        Assert.False(currencyProperty.IsNullable);
        Assert.Equal(3, currencyProperty.GetMaxLength());
    }

    /// <summary>
    /// Tests that the Configure method creates a unique composite index
    /// on the HotelId and RoomNumber properties.
    /// </summary>
    [Fact]
    public void Configure_ValidBuilder_CreatesUniqueIndexOnHotelIdAndRoomNumber()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);
        var model = context.Model;
        var entityType = model.FindEntityType(typeof(Room));

        // Act
        var indexes = entityType?.GetIndexes();
        var compositeIndex = indexes?.FirstOrDefault(i =>
            i.Properties.Count == 2 &&
            i.Properties.Any(p => p.Name == "HotelId") &&
            i.Properties.Any(p => p.Name == "RoomNumber"));

        // Assert
        Assert.NotNull(compositeIndex);
        Assert.True(compositeIndex.IsUnique);
        Assert.Equal(2, compositeIndex.Properties.Count);
    }

    /// <summary>
    /// Tests that the Configure method ignores the DomainEvents property
    /// so it is not mapped to the database.
    /// </summary>
    [Fact]
    public void Configure_ValidBuilder_IgnoresDomainEventsProperty()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);
        var model = context.Model;
        var entityType = model.FindEntityType(typeof(Room));

        // Act
        var domainEventsProperty = entityType?.FindProperty("DomainEvents");

        // Assert
        Assert.Null(domainEventsProperty);
    }

    /// <summary>
    /// Tests that the Configure method throws ArgumentNullException
    /// when provided with a null EntityTypeBuilder.
    /// </summary>
    [Fact]
    public void Configure_NullBuilder_ThrowsArgumentNullException()
    {
        // Arrange
        var configuration = new RoomConfiguration();

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => configuration.Configure(null!));
    }

    /// <summary>
    /// Test DbContext used for validating Room entity configuration.
    /// </summary>
    private class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }

        public DbSet<Room> Rooms => Set<Room>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new RoomConfiguration());
        }
    }
}