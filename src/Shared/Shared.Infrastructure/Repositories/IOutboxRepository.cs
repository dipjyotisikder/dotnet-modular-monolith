using Shared.Domain.Repositories;
using Shared.Infrastructure.Entities;

namespace Shared.Infrastructure.Repositories;

public interface IOutboxRepository : IRepository<OutboxMessage> { }
