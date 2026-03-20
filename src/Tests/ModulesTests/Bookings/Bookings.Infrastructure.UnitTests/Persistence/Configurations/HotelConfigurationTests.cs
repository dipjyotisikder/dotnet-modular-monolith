using Moq;
using System.Linq.Expressions;


namespace Bookings.Infrastructure.UnitTests.Persistence.Configurations;

/// <summary>
/// Unit tests for the HotelConfiguration class.
/// </summary>
public class HotelConfigurationTests
{
    /// <summary>
    /// Tests that Configure method calls HasKey to set up the primary key.
    /// Verifies that the Id property is configured as the primary key.
    /// </summary>
    [Fact]
    public void Configure_ValidBuilder_CallsHasKeyForId()
    {
        // Arrange
        var configuration = new HotelConfiguration();
        var modelBuilder = new ModelBuilder();
        var entityTypeBuilder = modelBuilder.Entity<Hotel>();

        // Act
        configuration.Configure(entityTypeBuilder);

        // Assert
        var entityType = modelBuilder.Model.FindEntityType(typeof(Hotel));
        var primaryKey = entityType.FindPrimaryKey();
        Assert.NotNull(primaryKey);
        Assert.Single(primaryKey.Properties);
        Assert.Equal("Id", primaryKey.Properties.First().Name);
    }

    /// <summary>
    /// Tests that Configure method configures the Name property with required and max length constraints.
    /// Verifies that Name property configuration methods are called correctly.
    /// </summary>
    [Fact]
    public void Configure_ValidBuilder_ConfiguresNamePropertyAsRequiredWithMaxLength()
    {
        // Arrange
        var configuration = new HotelConfiguration();
        var modelBuilder = new ModelBuilder();
        var entityTypeBuilder = modelBuilder.Entity<Hotel>();

        // Act
        configuration.Configure(entityTypeBuilder);

        // Assert
        var model = modelBuilder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(Hotel));
        Assert.NotNull(entityType);

        var nameProperty = entityType.FindProperty(nameof(Hotel.Name));
        Assert.NotNull(nameProperty);
        Assert.False(nameProperty.IsNullable);
        Assert.Equal(200, nameProperty.GetMaxLength());
    }

    /// <summary>
    /// Tests that Configure method configures the Address as an owned entity.
    /// Verifies that OwnsOne is called for the Address property.
    /// </summary>
    [Fact]
    public void Configure_ValidBuilder_ConfiguresAddressAsOwnedEntity()
    {
        // Arrange
        var configuration = new HotelConfiguration();
        var modelBuilder = new ModelBuilder();
        var entityTypeBuilder = modelBuilder.Entity<Hotel>();

        // Act
        configuration.Configure(entityTypeBuilder);

        // Assert
        var model = modelBuilder.FinalizeModel();
        var hotelEntityType = model.FindEntityType(typeof(Hotel));
        Assert.NotNull(hotelEntityType);

        var addressNavigation = hotelEntityType.FindNavigation(nameof(Hotel.Address));
        Assert.NotNull(addressNavigation);
        Assert.True(addressNavigation.ForeignKey.IsOwnership, "Address should be configured as an owned entity");
    }

    /// <summary>
    /// Tests that Configure method sets up the relationship between Hotel and Rooms.
    /// Verifies that HasMany is called for the Rooms collection.
    /// </summary>
    [Fact]
    public void Configure_ValidBuilder_ConfiguresRoomsRelationship()
    {
        // Arrange
        var configuration = new HotelConfiguration();
        var modelBuilder = new ModelBuilder();
        var entityTypeBuilder = modelBuilder.Entity<Hotel>();

        // Act
        configuration.Configure(entityTypeBuilder);

        // Assert
        var model = modelBuilder.FinalizeModel();
        var hotelEntityType = model.FindEntityType(typeof(Hotel));
        Assert.NotNull(hotelEntityType);

        var roomsNavigation = hotelEntityType.FindNavigation(nameof(Hotel.Rooms));
        Assert.NotNull(roomsNavigation);
        Assert.True(roomsNavigation.IsCollection);
        Assert.Equal(typeof(Room), roomsNavigation.TargetEntityType.ClrType);
    }

    /// <summary>
    /// Tests that Configure method sets cascade delete behavior for the Rooms relationship.
    /// Verifies that OnDelete is called with DeleteBehavior.Cascade.
    /// </summary>
    [Fact]
    public void Configure_ValidBuilder_ConfiguresCascadeDeleteForRooms()
    {
        // Arrange
        var configuration = new HotelConfiguration();
        var optionsBuilder = new DbContextOptionsBuilder<DbContext>();
        optionsBuilder.UseInMemoryDatabase("TestDb");
        var modelBuilder = new ModelBuilder(new Microsoft.EntityFrameworkCore.Metadata.Conventions.ConventionSet());
        var entityTypeBuilder = modelBuilder.Entity<Hotel>();

        // Act
        configuration.Configure(entityTypeBuilder);

        // Assert
        var model = modelBuilder.FinalizeModel();
        var hotelEntityType = model.FindEntityType(typeof(Hotel));
        var roomsNavigation = hotelEntityType.FindNavigation(nameof(Hotel.Rooms));
        var foreignKey = roomsNavigation.ForeignKey;

        Assert.Equal(DeleteBehavior.Cascade, foreignKey.DeleteBehavior);
    }

    /// <summary>
    /// Tests that Configure method sets the property access mode for Rooms navigation to Field.
    /// Verifies that UsePropertyAccessMode is called with PropertyAccessMode.Field.
    /// </summary>
    [Fact]
    public void Configure_ValidBuilder_SetsPropertyAccessModeForRoomsToField()
    {
        // Arrange
        var configuration = new HotelConfiguration();
        var modelBuilder = new ModelBuilder();
        var entityTypeBuilder = modelBuilder.Entity<Hotel>();

        // Act
        configuration.Configure(entityTypeBuilder);

        // Assert
        var model = modelBuilder.FinalizeModel();
        var hotelEntityType = model.FindEntityType(typeof(Hotel));
        Assert.NotNull(hotelEntityType);
        var roomsNavigation = hotelEntityType.FindNavigation(nameof(Hotel.Rooms));
        Assert.NotNull(roomsNavigation);
        Assert.Equal(PropertyAccessMode.Field, roomsNavigation.GetPropertyAccessMode());
    }

    /// <summary>
    /// Helper method to set up common mock builder behavior.
    /// </summary>
    private static void SetupMockBuilder(Mock<EntityTypeBuilder<Hotel>> mockBuilder)
    {
        var mockKeyBuilder = new Mock<KeyBuilder>();
        var mockStringPropertyBuilder = new Mock<PropertyBuilder<string>>();
        var mockIntPropertyBuilder = new Mock<PropertyBuilder<int>>();
        var mockBoolPropertyBuilder = new Mock<PropertyBuilder<bool>>();
        var mockDateTimePropertyBuilder = new Mock<PropertyBuilder<DateTime>>();
        var mockCollectionNavigationBuilder = new Mock<CollectionNavigationBuilder<Hotel, Room>>();
        var mockReferenceCollectionBuilder = new Mock<ReferenceCollectionBuilder<Hotel, Room>>();
        var mockNavigationBuilder = new Mock<NavigationBuilder<Hotel, System.Collections.Generic.IReadOnlyList<Room>>>();

        mockBuilder.Setup(b => b.HasKey(It.IsAny<Expression<Func<Hotel, object?>>>()))
            .Returns(mockKeyBuilder.Object);

        mockBuilder.Setup(b => b.Property(It.IsAny<Expression<Func<Hotel, string>>>()))
            .Returns(mockStringPropertyBuilder.Object);
        mockStringPropertyBuilder.Setup(p => p.IsRequired(It.IsAny<bool>()))
            .Returns(mockStringPropertyBuilder.Object);
        mockStringPropertyBuilder.Setup(p => p.HasMaxLength(It.IsAny<int>()))
            .Returns(mockStringPropertyBuilder.Object);

        mockBuilder.Setup(b => b.Property(It.IsAny<Expression<Func<Hotel, int>>>()))
            .Returns(mockIntPropertyBuilder.Object);
        mockIntPropertyBuilder.Setup(p => p.IsRequired(It.IsAny<bool>()))
            .Returns(mockIntPropertyBuilder.Object);

        mockBuilder.Setup(b => b.Property(It.IsAny<Expression<Func<Hotel, bool>>>()))
            .Returns(mockBoolPropertyBuilder.Object);
        mockBoolPropertyBuilder.Setup(p => p.IsRequired(It.IsAny<bool>()))
            .Returns(mockBoolPropertyBuilder.Object);

        mockBuilder.Setup(b => b.Property(It.IsAny<Expression<Func<Hotel, DateTime>>>()))
            .Returns(mockDateTimePropertyBuilder.Object);
        mockDateTimePropertyBuilder.Setup(p => p.IsRequired(It.IsAny<bool>()))
            .Returns(mockDateTimePropertyBuilder.Object);

        mockBuilder.Setup(b => b.OwnsOne(
                It.IsAny<Expression<Func<Hotel, Address?>>>(),
                It.IsAny<Action<OwnedNavigationBuilder<Hotel, Address>>>()))
            .Returns(mockBuilder.Object);

        mockBuilder.Setup(b => b.HasMany<Room>(It.IsAny<Expression<Func<Hotel, System.Collections.Generic.IEnumerable<Room>?>>>()))
            .Returns(mockCollectionNavigationBuilder.Object);
        mockCollectionNavigationBuilder.Setup(c => c.WithOne(It.IsAny<string>()))
            .Returns(mockReferenceCollectionBuilder.Object);
        mockReferenceCollectionBuilder.Setup(r => r.HasForeignKey(It.IsAny<Expression<Func<Room, object?>>>()))
            .Returns(mockReferenceCollectionBuilder.Object);
        mockReferenceCollectionBuilder.Setup(r => r.OnDelete(It.IsAny<DeleteBehavior>()))
            .Returns(mockReferenceCollectionBuilder.Object);

        mockBuilder.Setup(b => b.Navigation(It.IsAny<Expression<Func<Hotel, System.Collections.Generic.IReadOnlyList<Room>?>>>()))
            .Returns(mockNavigationBuilder.Object);
        mockNavigationBuilder.Setup(n => n.UsePropertyAccessMode(It.IsAny<PropertyAccessMode>()))
            .Returns(mockNavigationBuilder.Object);

        mockBuilder.Setup(b => b.Ignore(It.IsAny<Expression<Func<Hotel, object?>>>()))
            .Returns(mockBuilder.Object);
    }
}