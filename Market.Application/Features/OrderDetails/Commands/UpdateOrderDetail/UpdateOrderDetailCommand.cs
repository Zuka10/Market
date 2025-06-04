using Market.Application.Common.Interfaces;

namespace Market.Application.Features.OrderDetails.Commands.UpdateOrderDetail;

public record UpdateOrderDetailCommand(
    long OrderDetailId,
    long ProductId,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal,
    decimal CostPrice,
    decimal Profit
) : ICommand<bool>;