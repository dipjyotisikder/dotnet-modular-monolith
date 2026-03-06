using Users.Domain.Entities;

namespace Users.Domain.Repositories;

public interface IUserDeviceRepository
{
    Task<UserDevice?> GetByDeviceIdAsync(string deviceId, CancellationToken cancellationToken = default);
    Task<UserDevice?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserDevice>> GetUserDevicesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserDevice>> GetActiveUserDevicesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(UserDevice device, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserDevice device, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid deviceId, CancellationToken cancellationToken = default);
    Task RevokeUserDevicesAsync(Guid userId, CancellationToken cancellationToken = default);
}
