using FluentValidation;

namespace Market.Application.Features.Locations.Commands.ActivateLocation;

public class ActivateLocationValidator : AbstractValidator<ActivateLocationCommand>
{
    public ActivateLocationValidator()
    {
        RuleFor(x => x.LocationId)
            .GreaterThan(0)
            .WithMessage("Location ID is required.");
    }
}