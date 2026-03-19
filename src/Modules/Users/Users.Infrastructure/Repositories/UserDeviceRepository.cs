using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Repositories;
using Users.Domain.Entities;
using Users.Domain.Repositories;
using Users.Infrastructure.Persistence;

namespace Users.Infrastructure.Repositories;

public class UserDeviceRepository : Repository<UserDevice>, IUserDeviceRepository
{
    private readonly UsersDbContext _dbContext;

    public UserDeviceRepository(UsersDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserDevice?> GetByDeviceIdAsync(string deviceId, CancellationToken cancellationToken = default)
        => await _dbContext.UserDevices
            .FirstOrDefaultAsync(d => d.DeviceId == deviceId, cancellationToken);

    public async Task<UserDevice?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        => await _dbContext.UserDevices
            .FirstOrDefaultAsync(d => d.RefreshToken == refreshToken, cancellationToken);

    public async Task<IEnumerable<UserDevice>> GetUserDevicesAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _dbContext.UserDevices
            .Where(d => d.UserId == userId)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<UserDevice>> GetActiveUserDevicesAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _dbContext.UserDevices
            .Where(d => d.UserId == userId && !d.IsRevoked && d.RefreshTokenExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);

    public async Task UpdateAsync(UserDevice device, CancellationToken cancellationToken = default)
    {
        Update(device);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid deviceId, CancellationToken cancellationToken = default)
    {
        var device = await _dbContext.UserDevices.FindAsync(new object[] { deviceId }, cancellationToken: cancellationToken);
        if (device != null)
            Remove(device);
        await Task.CompletedTask;
    }

    public async Task RevokeUserDevicesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var devices = await GetUserDevicesAsync(userId, cancellationToken);
        foreach (var device in devices)
        {
            device.Revoke("User tokens revoked");
            Update(device);
        }
    }
}
