using Market.Application.Common.Interfaces;

namespace Market.Application.Features.Orders.Commands.CancelOrder;

public record CancelOrderCommand(long OrderId) : ICommand<bool>;