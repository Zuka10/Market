using Market.Application.Common.Interfaces;
using Market.Domain.Enums;

namespace Market.Application.Features.Orders.Commands.UpdateOrderStatus;

public record UpdateOrderStatusCommand(
    long OrderId,
    OrderStatus NewStatus
) : ICommand<bool>;