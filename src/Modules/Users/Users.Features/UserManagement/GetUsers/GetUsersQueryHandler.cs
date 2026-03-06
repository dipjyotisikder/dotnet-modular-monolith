using MediatR;
using Shared.Domain;
using Users.Domain.Repositories;

namespace Users.Features.UserManagement.GetUsers;

public class GetUsersQueryHandler(
    IUserRepository userRepository)
    : IRequestHandler<GetUsersQuery, Result<IEnumerable<GetUsersResponse>>>
{
    public async Task<Result<IEnumerable<GetUsersResponse>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var allUsers = await userRepository.GetAllAsync(cancellationToken);
            var users = allUsers.Select(u => new GetUsersResponse(
                u.Id.ToString(),
                u.Email,
                u.Name,
                u.Tier,
                u.CreatedAt,
                u.IsActive)).ToList();

            return Result.Success(users.AsEnumerable());
        }
        catch (Exception)
        {
            return Result.Failure<IEnumerable<GetUsersResponse>>("An error occurred while retrieving users", ErrorCodes.INTERNAL_ERROR);
        }
    }
}
