using FluentValidation;

namespace Market.Application.Features.Orders.Commands.ApplyDiscountToOrder;

public class ApplyDiscountToOrderValidator : AbstractValidator<ApplyDiscountToOrderCommand>
{
    public ApplyDiscountToOrderValidator()
    {
        RuleFor(x => x.OrderId)
            .GreaterThan(0).WithMessage("Order ID must be greater than 0.");

        RuleFor(x => x.DiscountId)
            .GreaterThan(0).WithMessage("Discount ID must be greater than 0.")
            .When(x => x.DiscountId.HasValue);

        RuleFor(x => x.DiscountAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Discount amount cannot be negative.")
            .LessThan(100000).WithMessage("Discount amount cannot exceed $100,000.");
    }
}