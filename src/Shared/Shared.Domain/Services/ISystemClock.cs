namespace Shared.Domain.Services;

public interface ISystemClock
{
    DateTime UtcNow { get; }

    DateTime UtcToday { get; }
}
