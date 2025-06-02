using FluentValidation;

namespace Market.Application.Features.Orders.Queries.GetOrderById;

public class GetOrderByIdValidator : AbstractValidator<GetOrderByIdQuery>
{
    public GetOrderByIdValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Order ID must be greater than 0.");
    }
}