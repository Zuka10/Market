using FluentValidation;

namespace Market.Application.Features.Payments.Commands.UpdatePayment;

public class UpdatePaymentValidator : AbstractValidator<UpdatePaymentCommand>
{
    public UpdatePaymentValidator()
    {
        RuleFor(x => x.PaymentId)
            .GreaterThan(0).WithMessage("Payment ID must be greater than 0.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Payment amount must be greater than 0.")
            .LessThan(1000000).WithMessage("Payment amount cannot exceed $1,000,000.");

        RuleFor(x => x.PaymentMethod)
            .IsInEnum().WithMessage("Invalid payment method.");

        RuleFor(x => x.PaymentDate)
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .WithMessage("Payment date cannot be more than 1 day in the future.")
            .GreaterThanOrEqualTo(DateTime.UtcNow.AddYears(-1))
            .WithMessage("Payment date cannot be more than 1 year in the past.");
    }
}