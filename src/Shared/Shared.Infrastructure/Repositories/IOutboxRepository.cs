using Shared.Infrastructure.Entities;
using Shared.Infrastructure.Repositories;

namespace Shared.Infrastructure.Repositories;

public interface IOutboxRepository : IRepository<OutboxMessage> { }
