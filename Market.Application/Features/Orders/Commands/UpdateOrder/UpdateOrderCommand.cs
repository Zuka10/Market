using Market.Application.Common.Interfaces;
using Market.Domain.Enums;

namespace Market.Application.Features.Orders.Commands.UpdateOrder;

public record UpdateOrderCommand(
    long OrderId,
    string OrderNumber,
    DateTime OrderDate,
    decimal Total,
    decimal SubTotal,
    decimal TotalCommission,
    OrderStatus Status,
    long LocationId,
    long? DiscountId,
    decimal DiscountAmount,
    long UserId,
    string? CustomerName,
    string? CustomerPhone,
    string? Notes
) : ICommand<bool>;