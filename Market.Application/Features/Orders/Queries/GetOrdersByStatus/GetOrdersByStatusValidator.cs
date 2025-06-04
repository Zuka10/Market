using FluentValidation;

namespace Market.Application.Features.Orders.Queries.GetOrdersByStatus;

public class GetOrdersByStatusValidator : AbstractValidator<GetOrdersByStatusQuery>
{
    public GetOrdersByStatusValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid order status.");
    }
}