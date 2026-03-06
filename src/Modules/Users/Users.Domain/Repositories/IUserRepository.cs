using Shared.Infrastructure.Repositories;
using Users.Domain.Entities;

namespace Users.Domain.Repositories;

public interface IUserRepository : IRepository<User>
{
}
