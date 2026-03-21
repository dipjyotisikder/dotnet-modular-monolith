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
        if (!Guid.TryParse(request.UserId, out var userId))
            return Result.ValidationError<GetCurrentUserResponse>("Invalid user ID");

        var users = await userRepository.FindAsync(u => u.Id == userId, cancellationToken);
        var user = users.FirstOrDefault();

        if (user == null)
            return Result.NotFound<GetCurrentUserResponse>("User not found");

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
}
