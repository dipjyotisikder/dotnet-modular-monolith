using Shared.Domain.Entities;
using Shared.Infrastructure.Repositories;

namespace Shared.Domain.Repositories;

public interface IOutboxRepository : IRepository<OutboxMessage> { }
