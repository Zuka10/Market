using Market.Application.Common.Models;
using MediatR;

namespace Market.Application.Common.Interfaces;

public interface ICommand : IRequest<BaseResponse<Unit>>
{
}

public interface ICommand<TResponse> : IRequest<BaseResponse<TResponse>>
{
}