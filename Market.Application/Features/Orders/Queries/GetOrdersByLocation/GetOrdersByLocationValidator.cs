using FluentValidation;

namespace Market.Application.Features.Orders.Queries.GetOrdersByLocation;

public class GetOrdersByLocationValidator : AbstractValidator<GetOrdersByLocationQuery>
{
    public GetOrdersByLocationValidator()
    {
        RuleFor(x => x.LocationId)
            .GreaterThan(0).WithMessage("Location ID must be greater than 0.");
    }
}