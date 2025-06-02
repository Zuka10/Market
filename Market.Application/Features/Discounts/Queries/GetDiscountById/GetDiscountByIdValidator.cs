using FluentValidation;

namespace Market.Application.Features.Discounts.Queries.GetDiscountById;

public class GetDiscountByIdValidator : AbstractValidator<GetDiscountByIdQuery>
{
    public GetDiscountByIdValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Discount ID must be greater than 0.");
    }
}