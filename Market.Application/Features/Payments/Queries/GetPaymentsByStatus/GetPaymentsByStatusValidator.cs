using FluentValidation;

namespace Market.Application.Features.Payments.Queries.GetPaymentsByStatus;

public class GetPaymentsByStatusValidator : AbstractValidator<GetPaymentsByStatusQuery>
{
    public GetPaymentsByStatusValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid payment status.");
    }
}