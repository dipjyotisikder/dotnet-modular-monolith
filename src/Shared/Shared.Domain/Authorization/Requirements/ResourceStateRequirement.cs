namespace Shared.Domain.Authorization.Requirements;

public class ResourceStateRequirement : IAuthorizationRequirement
{
    public string CurrentState { get; set; }
    public IReadOnlySet<string> AllowedStates { get; set; }
    public string? FailureReason { get; set; }

    public ResourceStateRequirement(string currentState, IReadOnlySet<string> allowedStates)
    {
        CurrentState = currentState;
        AllowedStates = allowedStates;
    }

    public ResourceStateRequirement(string currentState, IReadOnlySet<string> allowedStates, string failureReason)
    {
        CurrentState = currentState;
        AllowedStates = allowedStates;
        FailureReason = failureReason;
    }
}
