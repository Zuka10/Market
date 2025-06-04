using FluentValidation;

namespace Market.Application.Features.Discounts.Commands.ActivateDiscount;

public class ActivateDiscountValidator : AbstractValidator<ActivateDiscountCommand>
{
    public ActivateDiscountValidator()
    {
        RuleFor(x => x.DiscountId)
            .GreaterThan(0).WithMessage("Discount ID must be greater than 0.");
    }
}