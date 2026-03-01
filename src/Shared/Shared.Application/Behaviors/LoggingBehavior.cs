using MediatR;
using Microsoft.Extensions.Logging;

namespace Shared.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var requestNameWithModule = $"{typeof(TRequest).Namespace}.{requestName}";

        logger.LogInformation("Starting request: {RequestName}", requestNameWithModule);

        try
        {
            var response = await next();
            logger.LogInformation("Completed request: {RequestName}", requestNameWithModule);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception occurred during request: {RequestName}", requestNameWithModule);
            throw;
        }
    }
}
