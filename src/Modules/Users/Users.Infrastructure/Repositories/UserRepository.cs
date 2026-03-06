using Shared.Infrastructure.Repositories;
using Users.Domain.Entities;
using Users.Domain.Repositories;
using Users.Infrastructure.Persistence;

namespace Users.Infrastructure.Repositories;

public class UserRepository(UsersDbContext context) : Repository<User>(context), IUserRepository
{
}
