using MediatR;

namespace Market.Application.Common.Interfaces;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
        where TQuery : IQuery<TResponse>
{
}