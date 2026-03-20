using Bookings.Domain.Entities;
using Bookings.Domain.Enums;
using Bookings.Domain.ValueObjects;
using Shared.Domain;

namespace Bookings.Domain.UnitTests.Entities;

public class HotelTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void Create_ValidParameters_ReturnsSuccessResult(int starRating)
    {
        var name = "Grand Hotel";
        var description = "A luxurious hotel";
        var addressResult = Address.Create("123 Main St", "New York", "USA", "10001");
        var address = addressResult.Value;

        var result = Hotel.Create(name, description, starRating, address);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(name, result.Value.Name);
        Assert.Equal(description, result.Value.Description);
        Assert.Equal(starRating, result.Value.StarRating);
        Assert.Equal(address, result.Value.Address);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
        Assert.True(result.Value.IsActive);
        Assert.NotNull(result.Value.Rooms);
        Assert.Empty(result.Value.Rooms);
    }

    [Fact]
    public void Create_NameAndDescriptionWithWhitespace_TrimsWhitespace()
    {
        var nameWithWhitespace = "  Grand Hotel  ";
        var descriptionWithWhitespace = "\tA luxurious hotel\n";
        var addressResult = Address.Create("123 Main St", "New York", "USA", "10001");
        var address = addressResult.Value;

        var result = Hotel.Create(nameWithWhitespace, descriptionWithWhitespace, 3, address);

        Assert.True(result.IsSuccess);
        Assert.Equal("Grand Hotel", result.Value.Name);
        Assert.Equal("A luxurious hotel", result.Value.Description);
    }

    [Fact]
    public void Create_ValidParameters_SetsCreatedAtToUtcNow()
    {
        var name = "Grand Hotel";
        var description = "A luxurious hotel";
        var addressResult = Address.Create("123 Main St", "New York", "USA", "10001");
        var address = addressResult.Value;
        var beforeCreation = DateTime.UtcNow.AddSeconds(-1);

        var result = Hotel.Create(name, description, 3, address);
        var afterCreation = DateTime.UtcNow.AddSeconds(1);

        Assert.True(result.IsSuccess);
        Assert.InRange(result.Value.CreatedAt, beforeCreation, afterCreation);
    }

    [Theory]
    [InlineData(null, "null")]
    [InlineData("", "empty")]
    [InlineData(" ", "single space")]
    [InlineData("   ", "multiple spaces")]
    [InlineData("\t", "tab")]
    [InlineData("\n", "newline")]
    [InlineData("\r\n", "carriage return newline")]
    [InlineData("  \t\n  ", "mixed whitespace")]
    public void Create_InvalidName_ReturnsFailureResult(string? invalidName, string testCase)
    {
        var description = "A luxurious hotel";
        var addressResult = Address.Create("123 Main St", "New York", "USA", "10001");
        var address = addressResult.Value;

        var result = Hotel.Create(invalidName!, description, 3, address);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Hotel name cannot be empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(6)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void Create_InvalidStarRating_ReturnsFailureResult(int invalidStarRating)
    {
        var name = "Grand Hotel";
        var description = "A luxurious hotel";
        var addressResult = Address.Create("123 Main St", "New York", "USA", "10001");
        var address = addressResult.Value;

        var result = Hotel.Create(name, description, invalidStarRating, address);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Star rating must be between 1 and 5", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    [Fact]
    public void Create_EmptyDescription_ReturnsSuccessResult()
    {
        var name = "Grand Hotel";
        var emptyDescription = "";
        var addressResult = Address.Create("123 Main St", "New York", "USA", "10001");
        var address = addressResult.Value;

        var result = Hotel.Create(name, emptyDescription, 3, address);

        Assert.True(result.IsSuccess);
        Assert.Equal(string.Empty, result.Value.Description);
    }

    [Fact]
    public void Create_WhitespaceOnlyDescription_ReturnsSuccessResultWithEmptyDescription()
    {
        var name = "Grand Hotel";
        var whitespaceDescription = "   \t\n   ";
        var addressResult = Address.Create("123 Main St", "New York", "USA", "10001");
        var address = addressResult.Value;

        var result = Hotel.Create(name, whitespaceDescription, 3, address);

        Assert.True(result.IsSuccess);
        Assert.Equal(string.Empty, result.Value.Description);
    }

    [Fact]
    public void Create_VeryLongStrings_ReturnsSuccessResult()
    {
        var longName = new string('A', 10000);
        var longDescription = new string('B', 10000);
        var addressResult = Address.Create("123 Main St", "New York", "USA", "10001");
        var address = addressResult.Value;

        var result = Hotel.Create(longName, longDescription, 3, address);

        Assert.True(result.IsSuccess);
        Assert.Equal(longName, result.Value.Name);
        Assert.Equal(longDescription, result.Value.Description);
    }

    [Fact]
    public void Create_SpecialCharactersInStrings_ReturnsSuccessResult()
    {
        var nameWithSpecialChars = "Hôtel & Café ★★★";
        var descriptionWithSpecialChars = "A hotel with special chars: @#$%^&*()";
        var addressResult = Address.Create("123 Main St", "New York", "USA", "10001");
        var address = addressResult.Value;

        var result = Hotel.Create(nameWithSpecialChars, descriptionWithSpecialChars, 3, address);

        Assert.True(result.IsSuccess);
        Assert.Equal(nameWithSpecialChars, result.Value.Name);
        Assert.Equal(descriptionWithSpecialChars, result.Value.Description);
    }

    [Fact]
    public void Create_InvalidNameAndInvalidStarRating_ReturnsNameValidationError()
    {
        var invalidName = "";
        var invalidStarRating = 0;
        var description = "A luxurious hotel";
        var addressResult = Address.Create("123 Main St", "New York", "USA", "10001");
        var address = addressResult.Value;

        var result = Hotel.Create(invalidName, description, invalidStarRating, address);

        Assert.False(result.IsSuccess);
        Assert.Equal("Hotel name cannot be empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    [Fact]
    public void Create_MultipleInvocations_GeneratesUniqueIds()
    {
        var name = "Grand Hotel";
        var description = "A luxurious hotel";
        var addressResult = Address.Create("123 Main St", "New York", "USA", "10001");
        var address = addressResult.Value;

        var result1 = Hotel.Create(name, description, 3, address);
        var result2 = Hotel.Create(name, description, 3, address);

        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        Assert.NotEqual(result1.Value.Id, result2.Value.Id);
    }

    private static Hotel CreateValidHotel()
    {
        var addressResult = Address.Create("123 Main St", "New York", "USA", "10001");
        var hotelResult = Hotel.Create("Test Hotel", "Test Description", 4, addressResult.Value);
        return hotelResult.Value;
    }

    private static Money CreateValidMoney(decimal amount = 100m)
    {
        return Money.Create(amount, "USD").Value;
    }

    [Fact]
    public void AddRoom_ValidParameters_ReturnsSuccessWithRoom()
    {
        var hotel = CreateValidHotel();
        var roomNumber = "101";
        var roomType = RoomType.Standard;
        var pricePerNight = CreateValidMoney(100m);
        var maxOccupancy = 2;
        var description = "Standard room with twin beds";

        var result = hotel.AddRoom(roomNumber, roomType, pricePerNight, maxOccupancy, description);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Single(hotel.Rooms);
        Assert.Equal("101", hotel.Rooms.First().RoomNumber);
    }

    [Fact]
    public void AddRoom_DescriptionIsNull_ReturnsSuccessWithRoom()
    {
        var hotel = CreateValidHotel();
        var roomNumber = "102";
        var roomType = RoomType.Deluxe;
        var pricePerNight = CreateValidMoney(150m);
        var maxOccupancy = 3;

        var result = hotel.AddRoom(roomNumber, roomType, pricePerNight, maxOccupancy, null);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Single(hotel.Rooms);
    }

    [Fact]
    public void AddRoom_DescriptionNotProvided_ReturnsSuccessWithRoom()
    {
        var hotel = CreateValidHotel();
        var roomNumber = "103";
        var roomType = RoomType.Suite;
        var pricePerNight = CreateValidMoney(200m);
        var maxOccupancy = 4;

        var result = hotel.AddRoom(roomNumber, roomType, pricePerNight, maxOccupancy);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Single(hotel.Rooms);
    }

    [Fact]
    public void AddRoom_DuplicateRoomNumber_ReturnsFailureWithDuplicateResourceError()
    {
        var hotel = CreateValidHotel();
        var roomNumber = "201";
        var roomType = RoomType.Standard;
        var pricePerNight = CreateValidMoney(100m);
        var maxOccupancy = 2;

        hotel.AddRoom(roomNumber, roomType, pricePerNight, maxOccupancy);

        var result = hotel.AddRoom(roomNumber, roomType, pricePerNight, maxOccupancy);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.DUPLICATE_RESOURCE, result.ErrorCode);
        Assert.Contains("201", result.Error);
        Assert.Contains("already exists", result.Error);
        Assert.Single(hotel.Rooms);
    }

    [Theory]
    [InlineData("A101", "a101")]
    [InlineData("B202", "b202")]
    [InlineData("C303", "C303")]
    [InlineData("D404", "d404")]
    public void AddRoom_DuplicateRoomNumberDifferentCase_ReturnsFailureWithDuplicateResourceError(string firstRoomNumber, string secondRoomNumber)
    {
        var hotel = CreateValidHotel();
        var roomType = RoomType.Standard;
        var pricePerNight = CreateValidMoney(100m);
        var maxOccupancy = 2;

        hotel.AddRoom(firstRoomNumber, roomType, pricePerNight, maxOccupancy);

        var result = hotel.AddRoom(secondRoomNumber, roomType, pricePerNight, maxOccupancy);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.DUPLICATE_RESOURCE, result.ErrorCode);
        Assert.Single(hotel.Rooms);
    }

    [Theory]
    [InlineData("301", " 301")]
    [InlineData("302", "302 ")]
    [InlineData("303", " 303 ")]
    [InlineData("304", "  304  ")]
    public void AddRoom_DuplicateRoomNumberWithWhitespace_ReturnsFailureWithDuplicateResourceError(string firstRoomNumber, string secondRoomNumber)
    {
        var hotel = CreateValidHotel();
        var roomType = RoomType.Deluxe;
        var pricePerNight = CreateValidMoney(120m);
        var maxOccupancy = 2;

        hotel.AddRoom(firstRoomNumber, roomType, pricePerNight, maxOccupancy);

        var result = hotel.AddRoom(secondRoomNumber, roomType, pricePerNight, maxOccupancy);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.DUPLICATE_RESOURCE, result.ErrorCode);
        Assert.Single(hotel.Rooms);
    }

    [Fact]
    public void AddRoom_EmptyRoomNumber_ReturnsFailureWithValidationError()
    {
        var hotel = CreateValidHotel();
        var roomNumber = "";
        var roomType = RoomType.Standard;
        var pricePerNight = CreateValidMoney(100m);
        var maxOccupancy = 2;

        var result = hotel.AddRoom(roomNumber, roomType, pricePerNight, maxOccupancy);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
        Assert.Contains("Room number cannot be empty", result.Error);
        Assert.Empty(hotel.Rooms);
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public void AddRoom_WhitespaceOnlyRoomNumber_ReturnsFailureWithValidationError(string roomNumber)
    {
        var hotel = CreateValidHotel();
        var roomType = RoomType.Standard;
        var pricePerNight = CreateValidMoney(100m);
        var maxOccupancy = 2;

        var result = hotel.AddRoom(roomNumber, roomType, pricePerNight, maxOccupancy);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
        Assert.Contains("Room number cannot be empty", result.Error);
        Assert.Empty(hotel.Rooms);
    }

    [Fact]
    public void AddRoom_MaxOccupancyZero_ReturnsFailureWithValidationError()
    {
        var hotel = CreateValidHotel();
        var roomNumber = "401";
        var roomType = RoomType.Standard;
        var pricePerNight = CreateValidMoney(100m);
        var maxOccupancy = 0;

        var result = hotel.AddRoom(roomNumber, roomType, pricePerNight, maxOccupancy);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
        Assert.Contains("Max occupancy must be at least 1", result.Error);
        Assert.Empty(hotel.Rooms);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    [InlineData(-100)]
    [InlineData(int.MinValue)]
    public void AddRoom_MaxOccupancyNegative_ReturnsFailureWithValidationError(int maxOccupancy)
    {
        var hotel = CreateValidHotel();
        var roomNumber = "402";
        var roomType = RoomType.Standard;
        var pricePerNight = CreateValidMoney(100m);

        var result = hotel.AddRoom(roomNumber, roomType, pricePerNight, maxOccupancy);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
        Assert.Contains("Max occupancy must be at least 1", result.Error);
        Assert.Empty(hotel.Rooms);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    public void AddRoom_ValidMaxOccupancyBoundaryValues_ReturnsSuccess(int maxOccupancy)
    {
        var hotel = CreateValidHotel();
        var roomNumber = $"ROOM{maxOccupancy}";
        var roomType = RoomType.Standard;
        var pricePerNight = CreateValidMoney(100m);

        var result = hotel.AddRoom(roomNumber, roomType, pricePerNight, maxOccupancy);

        Assert.True(result.IsSuccess);
        Assert.Equal(maxOccupancy, result.Value.MaxOccupancy);
    }

    [Fact]
    public void AddRoom_PricePerNightZero_ReturnsFailureWithValidationError()
    {
        var hotel = CreateValidHotel();
        var roomNumber = "501";
        var roomType = RoomType.Standard;
        var pricePerNight = CreateValidMoney(0m);
        var maxOccupancy = 2;

        var result = hotel.AddRoom(roomNumber, roomType, pricePerNight, maxOccupancy);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
        Assert.Contains("Price per night must be greater than zero", result.Error);
        Assert.Empty(hotel.Rooms);
    }

    [Theory]
    [InlineData(RoomType.Standard)]
    [InlineData(RoomType.Deluxe)]
    [InlineData(RoomType.Suite)]
    [InlineData(RoomType.Presidential)]
    public void AddRoom_AllValidRoomTypes_ReturnsSuccess(RoomType roomType)
    {
        var hotel = CreateValidHotel();
        var roomNumber = $"ROOM{(int)roomType}";
        var pricePerNight = CreateValidMoney(100m);
        var maxOccupancy = 2;

        var result = hotel.AddRoom(roomNumber, roomType, pricePerNight, maxOccupancy);

        Assert.True(result.IsSuccess);
        Assert.Equal(roomType, result.Value.RoomType);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(100)]
    [InlineData(-1)]
    public void AddRoom_InvalidRoomTypeEnumValue_ReturnsSuccess(int invalidRoomTypeValue)
    {
        var hotel = CreateValidHotel();
        var roomNumber = $"ROOM{invalidRoomTypeValue}";
        var roomType = (RoomType)invalidRoomTypeValue;
        var pricePerNight = CreateValidMoney(100m);
        var maxOccupancy = 2;

        var result = hotel.AddRoom(roomNumber, roomType, pricePerNight, maxOccupancy);

        Assert.True(result.IsSuccess);
        Assert.Equal(roomType, result.Value.RoomType);
    }

    [Fact]
    public void AddRoom_MultipleDifferentRooms_AllSuccessfullyAdded()
    {
        var hotel = CreateValidHotel();
        var pricePerNight = CreateValidMoney(100m);

        var result1 = hotel.AddRoom("601", RoomType.Standard, pricePerNight, 2);
        var result2 = hotel.AddRoom("602", RoomType.Deluxe, pricePerNight, 3);
        var result3 = hotel.AddRoom("603", RoomType.Suite, pricePerNight, 4);

        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        Assert.True(result3.IsSuccess);
        Assert.Equal(3, hotel.Rooms.Count);
        Assert.Contains(hotel.Rooms, r => r.RoomNumber == "601");
        Assert.Contains(hotel.Rooms, r => r.RoomNumber == "602");
        Assert.Contains(hotel.Rooms, r => r.RoomNumber == "603");
    }

    [Theory]
    [InlineData("A-101")]
    [InlineData("B_202")]
    [InlineData("C.303")]
    [InlineData("D#404")]
    public void AddRoom_RoomNumberWithSpecialCharacters_ReturnsSuccess(string roomNumber)
    {
        var hotel = CreateValidHotel();
        var roomType = RoomType.Standard;
        var pricePerNight = CreateValidMoney(100m);
        var maxOccupancy = 2;

        var result = hotel.AddRoom(roomNumber, roomType, pricePerNight, maxOccupancy);

        Assert.True(result.IsSuccess);
        Assert.Equal(roomNumber.Trim().ToUpperInvariant(), result.Value.RoomNumber);
    }

    [Fact]
    public void AddRoom_VeryLongRoomNumber_ReturnsSuccess()
    {
        var hotel = CreateValidHotel();
        var roomNumber = new string('A', 1000);
        var roomType = RoomType.Standard;
        var pricePerNight = CreateValidMoney(100m);
        var maxOccupancy = 2;

        var result = hotel.AddRoom(roomNumber, roomType, pricePerNight, maxOccupancy);

        Assert.True(result.IsSuccess);
        Assert.Equal(roomNumber.Trim().ToUpperInvariant(), result.Value.RoomNumber);
    }

    [Fact]
    public void AddRoom_EmptyDescription_ReturnsSuccess()
    {
        var hotel = CreateValidHotel();
        var roomNumber = "701";
        var roomType = RoomType.Standard;
        var pricePerNight = CreateValidMoney(100m);
        var maxOccupancy = 2;
        var description = "";

        var result = hotel.AddRoom(roomNumber, roomType, pricePerNight, maxOccupancy, description);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void AddRoom_WhitespaceOnlyDescription_ReturnsSuccess(string description)
    {
        var hotel = CreateValidHotel();
        var roomNumber = "702";
        var roomType = RoomType.Standard;
        var pricePerNight = CreateValidMoney(100m);
        var maxOccupancy = 2;

        var result = hotel.AddRoom(roomNumber, roomType, pricePerNight, maxOccupancy, description);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }

    [Fact]
    public void AddRoom_VeryLongDescription_ReturnsSuccess()
    {
        var hotel = CreateValidHotel();
        var roomNumber = "703";
        var roomType = RoomType.Standard;
        var pricePerNight = CreateValidMoney(100m);
        var maxOccupancy = 2;
        var description = new string('X', 10000);

        var result = hotel.AddRoom(roomNumber, roomType, pricePerNight, maxOccupancy, description);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }

    [Theory]
    [InlineData("Room with <special> characters")]
    [InlineData("Room with 'quotes' and \"double quotes\"")]
    [InlineData("Room with newline\ncharacter")]
    [InlineData("Room with unicode: 日本語")]
    public void AddRoom_DescriptionWithSpecialCharacters_ReturnsSuccess(string description)
    {
        var hotel = CreateValidHotel();
        var roomNumber = "704";
        var roomType = RoomType.Standard;
        var pricePerNight = CreateValidMoney(100m);
        var maxOccupancy = 2;

        var result = hotel.AddRoom(roomNumber, roomType, pricePerNight, maxOccupancy, description);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(1.00)]
    [InlineData(99.99)]
    [InlineData(1000.00)]
    [InlineData(9999999.99)]
    public void AddRoom_ValidPriceAmounts_ReturnsSuccess(decimal priceAmount)
    {
        var hotel = CreateValidHotel();
        var roomNumber = $"PRICE{priceAmount}";
        var roomType = RoomType.Standard;
        var pricePerNight = CreateValidMoney(priceAmount);
        var maxOccupancy = 2;

        var result = hotel.AddRoom(roomNumber, roomType, pricePerNight, maxOccupancy);

        Assert.True(result.IsSuccess);
        Assert.Equal(priceAmount, result.Value.PricePerNight.Amount);
    }
}
