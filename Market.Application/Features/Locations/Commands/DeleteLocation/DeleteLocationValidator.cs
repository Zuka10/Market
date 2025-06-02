using FluentValidation;

namespace Market.Application.Features.Locations.Commands.DeleteLocation;

public class DeleteLocationValidator : AbstractValidator<DeleteLocationCommand>
{
    public DeleteLocationValidator()
    {
        RuleFor(x => x.LocationId)
            .GreaterThan(0).WithMessage("Location ID must be greater than 0.");
    }
}