using FluentValidation;

namespace Market.Application.Features.Products.Queries.GetProductsByLocation;

public class GetProductsByLocationValidator : AbstractValidator<GetProductsByLocationQuery>
{
    public GetProductsByLocationValidator()
    {
        RuleFor(x => x.LocationId)
            .GreaterThan(0).WithMessage("Location ID must be greater than 0.");
    }
}