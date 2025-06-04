using FluentValidation;

namespace Market.Application.Features.Payments.Commands.UpdatePaymentStatus;

public class UpdatePaymentStatusValidator : AbstractValidator<UpdatePaymentStatusCommand>
{
    public UpdatePaymentStatusValidator()
    {
        RuleFor(x => x.PaymentId)
            .GreaterThan(0).WithMessage("Payment ID must be greater than 0.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid payment status.");
    }
}