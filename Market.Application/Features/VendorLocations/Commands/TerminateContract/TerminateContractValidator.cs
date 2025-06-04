using FluentValidation;

namespace Market.Application.Features.VendorLocations.Commands.TerminateContract;

public class TerminateContractValidator : AbstractValidator<TerminateContractCommand>
{
    public TerminateContractValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("ID must be greater than 0.");

        RuleFor(x => x.TerminationDate)
            .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Termination date cannot be in the past.")
            .When(x => x.TerminationDate.HasValue);

        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Reason));
    }
}