using FluentValidation;

namespace Market.Application.Features.Payments.Commands.CancelPayment;

public class CancelPaymentValidator : AbstractValidator<CancelPaymentCommand>
{
    public CancelPaymentValidator()
    {
        RuleFor(x => x.PaymentId)
            .GreaterThan(0).WithMessage("Payment ID must be greater than 0.");
    }
}