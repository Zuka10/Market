using FluentValidation;

namespace Market.Application.Features.OrderDetails.Commands.UpdateOrderDetail;

public class UpdateOrderDetailValidator : AbstractValidator<UpdateOrderDetailCommand>
{
    public UpdateOrderDetailValidator()
    {
        RuleFor(x => x.OrderDetailId)
            .GreaterThan(0).WithMessage("Order detail ID must be greater than 0.");

        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Product ID must be greater than 0.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0.")
            .LessThan(10000).WithMessage("Quantity cannot exceed 10,000.");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("Unit price must be greater than 0.")
            .LessThan(100000).WithMessage("Unit price cannot exceed $100,000.");

        RuleFor(x => x.LineTotal)
            .GreaterThan(0).WithMessage("Line total must be greater than 0.")
            .LessThan(1000000).WithMessage("Line total cannot exceed $1,000,000.");

        RuleFor(x => x.CostPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Cost price cannot be negative.")
            .LessThan(100000).WithMessage("Cost price cannot exceed $100,000.");

        RuleFor(x => x.Profit)
            .GreaterThanOrEqualTo(0).WithMessage("Profit cannot be negative.")
            .LessThan(1000000).WithMessage("Profit cannot exceed $1,000,000.");

        // Business rule: Line total should equal unit price × quantity
        RuleFor(x => x)
            .Must(x => Math.Abs(x.LineTotal - (x.UnitPrice * x.Quantity)) < 0.01m)
            .WithMessage("Line total must equal unit price × quantity.");

        // Business rule: Profit should equal line total - (cost price × quantity)
        RuleFor(x => x)
            .Must(x => Math.Abs(x.Profit - (x.LineTotal - (x.CostPrice * x.Quantity))) < 0.01m)
            .WithMessage("Profit must equal line total - (cost price × quantity).");
    }
}