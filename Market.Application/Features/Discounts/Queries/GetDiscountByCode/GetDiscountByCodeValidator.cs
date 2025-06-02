using FluentValidation;

namespace Market.Application.Features.Discounts.Queries.GetDiscountByCode;

public class GetDiscountByCodeValidator : AbstractValidator<GetDiscountByCodeQuery>
{
    public GetDiscountByCodeValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Discount code is required.")
            .MaximumLength(20).WithMessage("Discount code cannot exceed 20 characters.");
    }
}