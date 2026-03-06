using Shared.Infrastructure.Entities;
using Shared.Infrastructure.Persistence;

namespace Shared.Infrastructure.Repositories;

public class OutboxRepository(OutboxDbContext context) : Repository<OutboxMessage>(context), IOutboxRepository
{
}