using Shared.Domain.Authorization;

namespace Shared.Application.Behaviors;

public interface IPermissionRequired
{
    string Permission { get; }
}

public interface IRequirementRequired
{
    IAuthorizationRequirement[] Requirements { get; }
}
