using Market.Application.Common.Interfaces;

namespace Market.Application.Features.Orders.Commands.ApplyDiscountToOrder;

public record ApplyDiscountToOrderCommand(
    long OrderId,
    long? DiscountId,
    decimal DiscountAmount
) : ICommand<bool>;