using FluentValidation;

namespace Market.Application.Features.Orders.Queries.GetOrderByUser;

public class GetOrdersByUserValidator : AbstractValidator<GetOrdersByUserQuery>
{
    public GetOrdersByUserValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User ID must be greater than 0.");
    }
}