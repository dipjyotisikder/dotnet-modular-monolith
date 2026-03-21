using MediatR;
using Shared.Domain;
using Shared.Domain.Repositories;
using Users.Domain.Entities;
using Users.Domain.Repositories;
using Users.Domain.Services;

namespace Users.Features.Authentication.RegisterWithPassword;

public class RegisterWithPasswordCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RegisterWithPasswordCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RegisterWithPasswordCommand request, CancellationToken cancellationToken)
    {
        var existingUsers = await userRepository.FindAsync(u => u.Email == request.Email, cancellationToken);
        if (existingUsers.Any())
            return Result.DuplicateResource<Guid>("Email already exists");

        var passwordHash = passwordHasher.HashPassword(request.Password);

        var userResult = User.Create(request.Email, request.Name, passwordHash);
        if (userResult.IsFailure)
            return Result.Failure<Guid>(userResult.Error, userResult.ErrorCode);

        var user = userResult.Value;

        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(user.Id);
    }
}
