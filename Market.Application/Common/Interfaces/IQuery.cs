using Market.Application.Common.Models;
using MediatR;

namespace Market.Application.Common.Interfaces;

public interface IQuery<TResponse> : IRequest<BaseResponse<TResponse>>
{
}