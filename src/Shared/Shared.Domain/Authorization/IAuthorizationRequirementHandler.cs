namespace Shared.Domain.Authorization;

public interface IAuthorizationRequirementHandler<in TRequirement> where TRequirement : IAuthorizationRequirement
{
    Task<bool> HandleAsync(Guid userId, TRequirement requirement);
}
