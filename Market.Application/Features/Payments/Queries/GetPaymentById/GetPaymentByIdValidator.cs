using FluentValidation;

namespace Market.Application.Features.Payments.Queries.GetPaymentById;

public class GetPaymentByIdValidator : AbstractValidator<GetPaymentByIdQuery>
{
    public GetPaymentByIdValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Payment ID must be greater than zero.");
    }
}