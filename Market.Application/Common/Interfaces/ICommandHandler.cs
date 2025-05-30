using Market.Application.Common.Models;
using MediatR;

namespace Market.Application.Common.Interfaces;

public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, BaseResponse<Unit>>
        where TCommand : ICommand
{
}

public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, BaseResponse<TResponse>>
    where TCommand : ICommand<TResponse>
{
}