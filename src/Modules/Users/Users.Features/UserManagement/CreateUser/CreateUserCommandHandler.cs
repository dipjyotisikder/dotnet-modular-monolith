using MediatR;
using Shared.Domain;
using Users.Domain.Entities;
using Users.Domain.Repositories;

namespace Users.Features.UserManagement.CreateUser;

public class CreateUserCommandHandler(
    IUserRepository userRepository)
    : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existingUsers = await userRepository.FindAsync(u => u.Email == request.Email, cancellationToken);
            if (existingUsers.Any())
                return Result.Failure<Guid>("Email already exists", ErrorCodes.DUPLICATE_RESOURCE);

            var userResult = User.Create(request.Email, request.Name);
            if (userResult.IsFailure)
                return Result.Failure<Guid>(userResult.Error, userResult.ErrorCode);

            var user = userResult.Value;

            await userRepository.AddAsync(user, cancellationToken);

            return Result.Success(user.Id);
        }
        catch (Exception)
        {
            return Result.Failure<Guid>("An error occurred while creating user", ErrorCodes.INTERNAL_ERROR);
        }
    }
}
