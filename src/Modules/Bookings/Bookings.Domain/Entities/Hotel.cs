using Bookings.Domain.ValueObjects;
using Shared.Domain;

namespace Bookings.Domain.Entities;

public class Hotel : Entity
{
    private readonly List<Room> _rooms = [];

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public int StarRating { get; private set; }
    public Address Address { get; private set; } = null!;
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }

    public IReadOnlyList<Room> Rooms => _rooms.AsReadOnly();

    private Hotel() { }

    public static Result<Hotel> Create(
        string name,
        string description,
        int starRating,
        Address address)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Hotel>("Hotel name cannot be empty", ErrorCodes.VALIDATION_ERROR);

        if (starRating is < 1 or > 5)
            return Result.Failure<Hotel>("Star rating must be between 1 and 5", ErrorCodes.VALIDATION_ERROR);

        return Result.Success(new Hotel
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description.Trim(),
            StarRating = starRating,
            Address = address,
            CreatedAt = DateTime.UtcNow
        });
    }

    public Result<Room> AddRoom(
        string roomNumber,
        Enums.RoomType roomType,
        Money pricePerNight,
        int maxOccupancy,
        string? description = null)
    {
        if (_rooms.Any(r => r.RoomNumber == roomNumber.Trim().ToUpperInvariant()))
            return Result.Failure<Room>(
                $"Room number '{roomNumber}' already exists in this hotel",
                ErrorCodes.DUPLICATE_RESOURCE);

        var roomResult = Room.Create(Id, roomNumber, roomType, pricePerNight, maxOccupancy, description);
        if (roomResult.IsFailure)
            return Result.Failure<Room>(roomResult.Error, roomResult.ErrorCode);

        _rooms.Add(roomResult.Value);
        return Result.Success(roomResult.Value);
    }

    public Result Deactivate()
    {
        if (!IsActive)
            return Result.Failure("Hotel is already inactive", ErrorCodes.BUSINESS_RULE_VIOLATION);

        IsActive = false;
        return Result.Success();
    }
}
