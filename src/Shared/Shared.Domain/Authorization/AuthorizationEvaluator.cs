namespace Shared.Domain.Authorization;

public class RequirementEvaluator : IRequirementEvaluator
{
    private readonly IServiceProvider _serviceProvider;

    public RequirementEvaluator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<bool> EvaluateAsync<TRequirement>(Guid userId, TRequirement requirement) where TRequirement : IAuthorizationRequirement
    {
        var handlerType = typeof(IAuthorizationRequirementHandler<>).MakeGenericType(typeof(TRequirement));
        var handler = _serviceProvider.GetService(handlerType);

        if (handler is null)
            return false;

        var method = handlerType.GetMethod(nameof(IAuthorizationRequirementHandler<TRequirement>.HandleAsync));
        if (method is null)
            return false;

        var result = method.Invoke(handler, new object[] { userId, requirement });
        if (result is Task<bool> task)
            return await task;

        return false;
    }
}
