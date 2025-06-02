using FluentValidation;

namespace Market.Application.Features.Payments.Queries.GetPaymentsByOrder;

public class GetPaymentsByOrderValidator : AbstractValidator<GetPaymentsByOrderQuery>
{
    public GetPaymentsByOrderValidator()
    {
        RuleFor(x => x.OrderId)
            .GreaterThan(0).WithMessage("Order ID must be greater than 0.");
    }
}