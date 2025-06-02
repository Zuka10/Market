using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;
using Market.Domain.Enums;

namespace Market.Application.Features.Orders.Commands.CreateOrder;

public record CreateOrderCommand(
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
    string? Notes,
    List<OrderDetailDto> OrderDetails
) : ICommand<OrderDto>;