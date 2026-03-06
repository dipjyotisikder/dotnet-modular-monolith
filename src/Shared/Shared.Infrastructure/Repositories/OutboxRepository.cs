using Shared.Infrastructure.Entities;
using Shared.Infrastructure.Persistence;
using Shared.Infrastructure.Repositories;

namespace Shared.Infrastructure.Repositories;

public class OutboxRepository(OutboxDbContext context) : Repository<OutboxMessage>(context), IOutboxRepository
{
}