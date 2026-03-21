using MediatR;
using Shared.Domain;
using Shared.Domain.Repositories;
using Users.Domain.Entities;
using Users.Domain.Repositories;

namespace Users.Features.UserManagement.CreateUser;

public class CreateUserCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var existingUsers = await userRepository.FindAsync(u => u.Email == request.Email, cancellationToken);
        if (existingUsers.Any())
            return Result.DuplicateResource<Guid>("Email already exists");

        var userResult = User.Create(request.Email, request.Name);
        if (userResult.IsFailure)
            return Result.Failure<Guid>(userResult.Error, userResult.ErrorCode);

        var user = userResult.Value;

        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(user.Id);
    }
}
