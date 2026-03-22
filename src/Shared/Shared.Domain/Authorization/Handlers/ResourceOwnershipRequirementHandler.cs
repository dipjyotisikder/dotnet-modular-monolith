using Shared.Domain.Authorization.Requirements;

namespace Shared.Domain.Authorization.Handlers;

public class ResourceOwnershipRequirementHandler : IAuthorizationRequirementHandler<ResourceOwnershipRequirement>
{
    public Task<bool> HandleAsync(Guid userId, ResourceOwnershipRequirement requirement)
    {
        bool isAuthorized = userId == requirement.ResourceOwnerId;
        return Task.FromResult(isAuthorized);
    }
}
