using FluentValidation;

namespace Market.Application.Features.Orders.Commands.UpdateOrder;

public class UpdateOrderValidator : AbstractValidator<UpdateOrderCommand>
{
    public UpdateOrderValidator()
    {
        RuleFor(x => x.OrderId)
            .GreaterThan(0).WithMessage("Order ID must be greater than 0.");

        RuleFor(x => x.OrderNumber)
            .NotEmpty().WithMessage("Order number is required.")
            .MaximumLength(50).WithMessage("Order number cannot exceed 50 characters.");

        RuleFor(x => x.OrderDate)
            .NotEmpty().WithMessage("Order date is required.");

        RuleFor(x => x.Total)
            .GreaterThan(0).WithMessage("Total must be greater than 0.")
            .LessThan(1000000).WithMessage("Total cannot exceed $1,000,000.");

        RuleFor(x => x.SubTotal)
            .GreaterThanOrEqualTo(0).WithMessage("Subtotal cannot be negative.");

        RuleFor(x => x.TotalCommission)
            .GreaterThanOrEqualTo(0).WithMessage("Total commission cannot be negative.");

        RuleFor(x => x.DiscountAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Discount amount cannot be negative.");

        RuleFor(x => x)
            .Must(x => x.DiscountAmount <= x.SubTotal)
            .WithMessage("Discount amount cannot exceed subtotal.");

        RuleFor(x => x.LocationId)
            .GreaterThan(0).WithMessage("Location ID must be greater than 0.");

        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User ID must be greater than 0.");

        RuleFor(x => x.DiscountId)
            .GreaterThan(0).WithMessage("Discount ID must be greater than 0.")
            .When(x => x.DiscountId.HasValue);

        RuleFor(x => x.CustomerName)
            .MaximumLength(100).WithMessage("Customer name cannot exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.CustomerName));

        RuleFor(x => x.CustomerPhone)
            .MaximumLength(20).WithMessage("Customer phone cannot exceed 20 characters.")
            .When(x => !string.IsNullOrEmpty(x.CustomerPhone));

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}