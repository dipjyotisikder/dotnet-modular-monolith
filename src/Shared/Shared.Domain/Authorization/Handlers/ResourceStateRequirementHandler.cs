using Shared.Domain.Authorization;
using Shared.Domain.Authorization.Requirements;

namespace Shared.Domain.Authorization.Handlers;

public class ResourceStateRequirementHandler : IAuthorizationRequirementHandler<ResourceStateRequirement>
{
    public Task<bool> HandleAsync(Guid userId, ResourceStateRequirement requirement)
    {
        bool isAuthorized = requirement.AllowedStates.Contains(requirement.CurrentState);
        return Task.FromResult(isAuthorized);
    }
}
