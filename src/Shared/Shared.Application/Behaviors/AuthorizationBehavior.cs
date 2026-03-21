using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Domain.Authorization;
using Shared.Domain.Services;

namespace Shared.Application.Behaviors;

public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IPermissionService _permissionService;
    private readonly IRequirementEvaluator _requirementEvaluator;
    private readonly IUserContext _userContext;
    private readonly ILogger<AuthorizationBehavior<TRequest, TResponse>> _logger;

    public AuthorizationBehavior(
        IPermissionService permissionService,
        IRequirementEvaluator requirementEvaluator,
        IUserContext userContext,
        ILogger<AuthorizationBehavior<TRequest, TResponse>> logger)
    {
        _permissionService = permissionService;
        _requirementEvaluator = requirementEvaluator;
        _userContext = userContext;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not IPermissionRequired and not IRequirementRequired)
            return await next();

        try
        {
            if (request is IPermissionRequired permissionRequired)
            {
                if (!_permissionService.HasPermission(permissionRequired.Permission))
                {
                    _logger.LogWarning(
                        "Permission denied: {Permission} for user {UserId}",
                        permissionRequired.Permission,
                        _userContext.UserId);

                    throw new PermissionDeniedException(permissionRequired.Permission, _userContext.UserId.ToString());
                }
            }

            if (request is IRequirementRequired requirementRequired)
            {
                foreach (var requirement in requirementRequired.Requirements)
                {
                    var evaluateMethod = typeof(IRequirementEvaluator)
                        .GetMethod(nameof(IRequirementEvaluator.EvaluateAsync))
                        ?.MakeGenericMethod(requirement.GetType());

                    if (evaluateMethod != null)
                    {
                        var result = await (Task<bool>)evaluateMethod.Invoke(
                            _requirementEvaluator,
                            new object[] { _userContext.UserId, requirement })!;

                        if (!result)
                        {
                            _logger.LogWarning(
                                "Requirement failed: {RequirementType} for user {UserId}",
                                requirement.GetType().Name,
                                _userContext.UserId);

                            throw new RequirementFailedException(requirement.GetType().Name, _userContext.UserId.ToString());
                        }
                    }
                }
            }

            return await next();
        }
        catch (AuthorizationException ex)
        {
            _logger.LogWarning(
                "Authorization failed: {ErrorCode} for user {UserId}",
                ex.ErrorCode,
                _userContext.UserId);

            throw;
        }
    }
}
