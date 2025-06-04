using Market.Application.Common.Models;
using MediatR;

namespace Market.Application.Common.Interfaces;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, BaseResponse<TResponse>>
        where TQuery : IQuery<TResponse>
{
}