using FluentValidation;

namespace Market.Application.Features.Locations.Commands.UpdateLocation;

public class UpdateLocationValidator : AbstractValidator<UpdateLocationCommand>
{
    public UpdateLocationValidator()
    {
        RuleFor(x => x.LocationId)
            .GreaterThan(0).WithMessage("Location ID must be greater than 0.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MaximumLength(100)
            .WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.Address)
            .NotEmpty()
            .WithMessage("Address is required.")
            .MaximumLength(200)
            .WithMessage("Address must not exceed 200 characters.");

        RuleFor(x => x.PostalCode)
            .NotEmpty()
            .WithMessage("Postal code is required.")
            .MaximumLength(20)
            .WithMessage("Postal code must not exceed 20 characters.");

        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage("City is required.")
            .MaximumLength(100)
            .WithMessage("City must not exceed 100 characters.");

        RuleFor(x => x.Country)
            .NotEmpty()
            .WithMessage("Country is required.")
            .MaximumLength(100)
            .WithMessage("Country must not exceed 100 characters.");
    }
}