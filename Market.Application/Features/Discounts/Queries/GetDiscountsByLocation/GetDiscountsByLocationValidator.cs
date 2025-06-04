using FluentValidation;

namespace Market.Application.Features.Discounts.Queries.GetDiscountsByLocation;

public class GetDiscountsByLocationValidator : AbstractValidator<GetDiscountsByLocationQuery>
{
    public GetDiscountsByLocationValidator()
    {
        RuleFor(x => x.LocationId)
            .GreaterThan(0).WithMessage("Location ID must be greater than 0.");
    }
}