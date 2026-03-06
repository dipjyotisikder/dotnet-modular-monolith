using MediatR;
using Shared.Domain;
using Users.Domain.Repositories;

namespace Users.Features.UserManagement.GetCurrentUser;

public class GetCurrentUserQueryHandler(
    IUserRepository userRepository)
    : IRequestHandler<GetCurrentUserQuery, Result<GetCurrentUserResponse>>
{
    public async Task<Result<GetCurrentUserResponse>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (!Guid.TryParse(request.UserId, out var userId))
                return Result.Failure<GetCurrentUserResponse>("Invalid user ID", ErrorCodes.VALIDATION_ERROR);

            var users = await userRepository.FindAsync(u => u.Id == userId, cancellationToken);
            var user = users.FirstOrDefault();

            if (user == null)
                return Result.Failure<GetCurrentUserResponse>("User not found", ErrorCodes.VALIDATION_ERROR);

            var response = new GetCurrentUserResponse(
                user.Id.ToString(),
                user.Email,
                user.Name,
                user.Tier,
                user.GetRoles().ToArray(),
                user.CreatedAt,
                user.LastLoginAt);

            return Result.Success(response);
        }
        catch (Exception)
        {
            return Result.Failure<GetCurrentUserResponse>("An error occurred while retrieving user", ErrorCodes.INTERNAL_ERROR);
        }
    }
}
