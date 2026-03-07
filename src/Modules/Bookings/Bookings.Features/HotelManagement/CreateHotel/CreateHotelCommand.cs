using MediatR;
using Shared.Domain;

namespace Bookings.Features.HotelManagement.CreateHotel;

public record CreateHotelRequest(
    string Name,
    string Description,
    int StarRating,
    string Street,
    string City,
    string Country,
    string ZipCode = "");

public record CreateHotelCommand(
    string Name,
    string Description,
    int StarRating,
    string Street,
    string City,
    string Country,
    string ZipCode) : IRequest<Result<Guid>>;
