using FluentValidation;

namespace Market.Application.Features.VendorLocations.Commands.ExtendContract;

public class ExtendContractValidator : AbstractValidator<ExtendContractCommand>
{
    public ExtendContractValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("ID must be greater than 0.");

        RuleFor(x => x.NewEndDate)
            .NotEmpty().WithMessage("New end date is required.")
            .GreaterThan(DateTime.Today).WithMessage("New end date must be in the future.");
    }
}