using MediatR;

namespace Market.Application.Common.Interfaces;

public interface IQuery<out TResponse> : IRequest<TResponse>
{
}