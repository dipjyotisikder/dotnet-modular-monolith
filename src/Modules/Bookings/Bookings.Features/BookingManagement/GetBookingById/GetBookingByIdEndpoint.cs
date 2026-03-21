using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Domain.Services;
using Shared.Infrastructure.Endpoints;
using Shared.Infrastructure.Mappers;

namespace Bookings.Features.BookingManagement.GetBookingById;

public class GetBookingByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGroup("/api/bookings")
            .MapGet("/{bookingId:guid}", GetBookingByIdHandler)
            .WithName("GetBookingById")
            .RequireAuthorization()
            .WithTags("Bookings")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status403Forbidden);
    }

    private static async Task<IResult> GetBookingByIdHandler(
        Guid bookingId,
        IUserContext userContext,
        ISender sender,
        CancellationToken cancellationToken)
    {
        if (!userContext.IsAuthenticated)
            return Results.Unauthorized();

        var result = await sender.Send(
            new GetBookingByIdQuery(bookingId, userContext.UserId),
            cancellationToken);

        return ResultToHttpResponseMapper.MapToHttpResponse(result);
    }
}
