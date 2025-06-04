using FluentValidation;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.Orders.Commands.CreateOrder;

public class CreateOrderDetailValidator : AbstractValidator<OrderDetailDto>
{
    public CreateOrderDetailValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Product ID must be greater than 0.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0.")
            .LessThan(10000).WithMessage("Quantity cannot exceed 10,000.");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("Unit price must be greater than 0.")
            .LessThan(100000).WithMessage("Unit price cannot exceed $100,000.");

        RuleFor(x => x.LineTotal)
            .GreaterThan(0).WithMessage("Line total must be greater than 0.");

        RuleFor(x => x.CostPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Cost price cannot be negative.");

        RuleFor(x => x.Profit)
            .GreaterThanOrEqualTo(0).WithMessage("Profit cannot be negative.");

        RuleFor(x => x)
            .Must(x => Math.Abs(x.LineTotal - (x.UnitPrice * x.Quantity)) < 0.01m)
            .WithMessage("Line total must equal unit price × quantity.");
    }
}