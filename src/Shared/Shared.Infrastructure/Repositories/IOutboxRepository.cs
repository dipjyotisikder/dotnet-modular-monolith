using Shared.Infrastructure.Entities;

namespace Shared.Infrastructure.Repositories;

public interface IOutboxRepository : IRepository<OutboxMessage> { }
