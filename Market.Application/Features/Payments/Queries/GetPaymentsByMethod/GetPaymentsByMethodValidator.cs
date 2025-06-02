using FluentValidation;

namespace Market.Application.Features.Payments.Queries.GetPaymentsByMethod;

public class GetPaymentsByMethodValidator : AbstractValidator<GetPaymentsByMethodQuery>
{
    public GetPaymentsByMethodValidator()
    {
        RuleFor(x => x.PaymentMethod)
            .IsInEnum().WithMessage("Invalid payment method.");
    }
}