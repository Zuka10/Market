using FluentValidation;

namespace Market.Application.Features.Orders.Queries.GetOrderByOrderNumber;

public class GetOrderByNumberValidator : AbstractValidator<GetOrderByNumberQuery>
{
    public GetOrderByNumberValidator()
    {
        RuleFor(x => x.OrderNumber)
            .NotEmpty().WithMessage("Order number is required.")
            .MaximumLength(50).WithMessage("Order number cannot exceed 50 characters.");
    }
}