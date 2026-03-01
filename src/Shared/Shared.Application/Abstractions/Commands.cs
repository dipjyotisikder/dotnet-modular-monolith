using MediatR;
using Shared.Domain;

namespace Shared.Application.Abstractions;

public abstract record Query<TResponse> : IRequest<Result<TResponse>>;

public abstract record Command<TResponse> : IRequest<Result<TResponse>>;

public abstract record Command : IRequest<Result>;
