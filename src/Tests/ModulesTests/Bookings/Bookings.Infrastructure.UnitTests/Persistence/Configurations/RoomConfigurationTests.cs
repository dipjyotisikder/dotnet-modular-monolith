using Bookings.Domain.Entities;
using Bookings.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Bookings.Infrastructure.UnitTests.Persistence.Configurations;

public class RoomConfigurationTests
{
    [Fact]
    public void Configure_ValidBuilder_ConfiguresRoomEntityCorrectly()
    {
        var configuration = new RoomConfiguration();
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);
        var model = context.Model;
        var entityType = model.FindEntityType(typeof(Room));

        Assert.NotNull(entityType);
    }

    [Fact]
    public void Configure_ValidBuilder_SetsPrimaryKeyOnIdProperty()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);
        var model = context.Model;
        var entityType = model.FindEntityType(typeof(Room));

        var primaryKey = entityType?.FindPrimaryKey();

        Assert.NotNull(primaryKey);
        Assert.Single(primaryKey.Properties);
        Assert.Equal("Id", primaryKey.Properties[0].Name);
    }

    [Fact]
    public void Configure_ValidBuilder_ConfiguresHotelIdAsRequired()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);
        var model = context.Model;
        var entityType = model.FindEntityType(typeof(Room));

        var hotelIdProperty = entityType?.FindProperty("HotelId");

        Assert.NotNull(hotelIdProperty);
        Assert.False(hotelIdProperty.IsNullable);
    }

    [Fact]
    public void Configure_ValidBuilder_ConfiguresRoomNumberAsRequiredWithMaxLength()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);
        var model = context.Model;
        var entityType = model.FindEntityType(typeof(Room));

        var roomNumberProperty = entityType?.FindProperty("RoomNumber");

        Assert.NotNull(roomNumberProperty);
        Assert.False(roomNumberProperty.IsNullable);
        Assert.Equal(20, roomNumberProperty.GetMaxLength());
    }

    [Fact]
    public void Configure_ValidBuilder_ConfiguresRoomTypeAsRequiredWithStringConversionAndMaxLength()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);
        var model = context.Model;
        var entityType = model.FindEntityType(typeof(Room));

        var roomTypeProperty = entityType?.FindProperty("RoomType");

        Assert.NotNull(roomTypeProperty);
        Assert.False(roomTypeProperty.IsNullable);
        Assert.Equal(50, roomTypeProperty.GetMaxLength());
        var valueConverter = roomTypeProperty.GetValueConverter();
        if (valueConverter != null)
        {
            Assert.Equal(typeof(string), valueConverter.ProviderClrType);
        }
    }

    [Fact]
    public void Configure_ValidBuilder_ConfiguresMaxOccupancyAsRequired()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);
        var model = context.Model;
        var entityType = model.FindEntityType(typeof(Room));

        var maxOccupancyProperty = entityType?.FindProperty("MaxOccupancy");

        Assert.NotNull(maxOccupancyProperty);
        Assert.False(maxOccupancyProperty.IsNullable);
    }

    [Fact]
    public void Configure_ValidBuilder_ConfiguresDescriptionWithMaxLengthAndAllowsNull()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);
        var model = context.Model;
        var entityType = model.FindEntityType(typeof(Room));

        var descriptionProperty = entityType?.FindProperty("Description");

        Assert.NotNull(descriptionProperty);
        Assert.True(descriptionProperty.IsNullable);
        Assert.Equal(500, descriptionProperty.GetMaxLength());
    }

    [Fact]
    public void Configure_ValidBuilder_ConfiguresIsAvailableAsRequired()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);
        var model = context.Model;
        var entityType = model.FindEntityType(typeof(Room));

        var isAvailableProperty = entityType?.FindProperty("IsAvailable");

        Assert.NotNull(isAvailableProperty);
        Assert.False(isAvailableProperty.IsNullable);
    }

    [Fact]
    public void Configure_ValidBuilder_ConfiguresPricePerNightOwnedEntityCorrectly()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);
        var model = context.Model;
        var entityType = model.FindEntityType(typeof(Room));

        var ownedNavigation = entityType?.FindNavigation("PricePerNight");
        var ownedEntityType = ownedNavigation?.TargetEntityType;
        var amountProperty = ownedEntityType?.FindProperty("Amount");
        var currencyProperty = ownedEntityType?.FindProperty("Currency");

        Assert.NotNull(amountProperty);
        Assert.Equal("PricePerNightAmount", amountProperty.GetColumnName());
        Assert.False(amountProperty.IsNullable);
        Assert.Equal(18, amountProperty.GetPrecision());
        Assert.Equal(2, amountProperty.GetScale());

        Assert.NotNull(currencyProperty);
        Assert.Equal("PricePerNightCurrency", currencyProperty.GetColumnName());
        Assert.False(currencyProperty.IsNullable);
        Assert.Equal(3, currencyProperty.GetMaxLength());
    }

    [Fact]
    public void Configure_ValidBuilder_CreatesUniqueIndexOnHotelIdAndRoomNumber()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);
        var model = context.Model;
        var entityType = model.FindEntityType(typeof(Room));

        var indexes = entityType?.GetIndexes();
        var compositeIndex = indexes?.FirstOrDefault(i =>
            i.Properties.Count == 2 &&
            i.Properties.Any(p => p.Name == "HotelId") &&
            i.Properties.Any(p => p.Name == "RoomNumber"));

        Assert.NotNull(compositeIndex);
        Assert.True(compositeIndex.IsUnique);
        Assert.Equal(2, compositeIndex.Properties.Count);
    }

    [Fact]
    public void Configure_ValidBuilder_IgnoresDomainEventsProperty()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);
        var model = context.Model;
        var entityType = model.FindEntityType(typeof(Room));

        var domainEventsProperty = entityType?.FindProperty("DomainEvents");

        Assert.Null(domainEventsProperty);
    }

    [Fact]
    public void Configure_NullBuilder_ThrowsArgumentNullException()
    {
        var configuration = new RoomConfiguration();

        Assert.Throws<NullReferenceException>(() => configuration.Configure(null!));
    }

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
