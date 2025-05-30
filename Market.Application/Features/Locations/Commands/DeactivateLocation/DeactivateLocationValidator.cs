using FluentValidation;

namespace Market.Application.Features.Locations.Commands.DeactivateLocation;

public class DeactivateLocationValidator : AbstractValidator<DeactivateLocationCommand>
{
    public DeactivateLocationValidator()
    {
        RuleFor(x => x.LocationId)
            .GreaterThan(0)
            .WithMessage("Location ID is required.");
    }
}