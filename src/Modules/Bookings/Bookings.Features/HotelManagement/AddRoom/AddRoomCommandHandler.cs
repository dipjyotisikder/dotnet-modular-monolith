using Bookings.Domain.Repositories;
using Bookings.Domain.ValueObjects;
using MediatR;
using Shared.Domain;
using Shared.Domain.Repositories;

namespace Bookings.Features.HotelManagement.AddRoom;

public class AddRoomCommandHandler(IHotelRepository hotelRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<AddRoomCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(AddRoomCommand request, CancellationToken cancellationToken)
    {
        var hotel = await hotelRepository.GetWithRoomsAsync(request.HotelId, cancellationToken);
        if (hotel is null)
            return Result.NotFound<Guid>("Hotel not found");

        if (!hotel.IsActive)
            return Result.BusinessRuleViolation<Guid>("Cannot add rooms to an inactive hotel");

        var moneyResult = Money.Create(request.PricePerNight, request.Currency);
        if (moneyResult.IsFailure)
            return Result.Failure<Guid>(moneyResult.Error, moneyResult.ErrorCode);

        var roomResult = hotel.AddRoom(
            request.RoomNumber,
            request.RoomType,
            moneyResult.Value,
            request.MaxOccupancy,
            request.Description);

        if (roomResult.IsFailure)
            return Result.Failure<Guid>(roomResult.Error, roomResult.ErrorCode);

        hotelRepository.Update(hotel);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(roomResult.Value.Id);
    }
}
