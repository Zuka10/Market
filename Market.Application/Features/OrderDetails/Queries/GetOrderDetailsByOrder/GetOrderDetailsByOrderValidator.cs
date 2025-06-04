using FluentValidation;

namespace Market.Application.Features.OrderDetails.Queries.GetOrderDetailsByOrder;

public class GetOrderDetailsByOrderValidator : AbstractValidator<GetOrderDetailsByOrderQuery>
{
    public GetOrderDetailsByOrderValidator()
    {
        RuleFor(x => x.OrderId)
            .GreaterThan(0).WithMessage("Order ID must be greater than 0.");
    }
}