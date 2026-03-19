using Bookings.Domain.Entities;
using Bookings.Domain.Repositories;
using Bookings.Domain.ValueObjects;
using MediatR;
using Shared.Domain;
using Shared.Domain.Repositories;

namespace Bookings.Features.HotelManagement.CreateHotel;

public class CreateHotelCommandHandler(IHotelRepository hotelRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateHotelCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateHotelCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var addressResult = Address.Create(request.Street, request.City, request.Country, request.ZipCode);
            if (addressResult.IsFailure)
                return Result.Failure<Guid>(addressResult.Error, addressResult.ErrorCode);

            var hotelResult = Hotel.Create(request.Name, request.Description, request.StarRating, addressResult.Value);
            if (hotelResult.IsFailure)
                return Result.Failure<Guid>(hotelResult.Error, hotelResult.ErrorCode);

            await hotelRepository.AddAsync(hotelResult.Value, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(hotelResult.Value.Id);
        }
        catch (Exception)
        {
            return Result.Failure<Guid>("An error occurred while creating the hotel", ErrorCodes.INTERNAL_ERROR);
        }
    }
}
