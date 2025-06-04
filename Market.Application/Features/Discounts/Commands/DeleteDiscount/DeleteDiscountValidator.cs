using FluentValidation;

namespace Market.Application.Features.Discounts.Commands.DeleteDiscount;

public class DeleteDiscountValidator : AbstractValidator<DeleteDiscountCommand>
{
    public DeleteDiscountValidator()
    {
        RuleFor(x => x.DiscountId)
            .GreaterThan(0).WithMessage("Discount ID must be greater than 0.");
    }
}