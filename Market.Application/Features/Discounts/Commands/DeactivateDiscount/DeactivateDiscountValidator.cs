using FluentValidation;

namespace Market.Application.Features.Discounts.Commands.DeactivateDiscount;

public class DeactivateDiscountValidator : AbstractValidator<DeactivateDiscountCommand>
{
    public DeactivateDiscountValidator()
    {
        RuleFor(x => x.DiscountId)
            .GreaterThan(0).WithMessage("Discount ID must be greater than 0.");
    }
}