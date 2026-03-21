namespace Shared.Domain.Authorization;

public interface IRequirementEvaluator
{
    Task<bool> EvaluateAsync<TRequirement>(Guid userId, TRequirement requirement) where TRequirement : IAuthorizationRequirement;
}
