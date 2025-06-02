using FluentValidation;

namespace Market.Application.Features.Procurements.Queries.GetProcurementByReference;

public class GetProcurementByReferenceValidator : AbstractValidator<GetProcurementByReferenceQuery>
{
    public GetProcurementByReferenceValidator()
    {
        RuleFor(x => x.ReferenceNo)
            .NotEmpty().WithMessage("Reference number is required.")
            .MaximumLength(50).WithMessage("Reference number cannot exceed 50 characters.");
    }
}