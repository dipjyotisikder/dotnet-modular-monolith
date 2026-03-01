namespace Shared.Infrastructure.Services;

using Shared.Domain.Services;

public class SystemClock : ISystemClock
{
    public DateTime UtcNow => DateTime.UtcNow;

    public DateTime UtcToday => DateTime.UtcNow.Date;
}
