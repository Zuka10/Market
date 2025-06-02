using FluentValidation;

namespace Market.Application.Features.Procurements.Queries.GetProcurementsByLocation;

public class GetProcurementsByLocationValidator : AbstractValidator<GetProcurementsByLocationQuery>
{
    public GetProcurementsByLocationValidator()
    {
        RuleFor(x => x.LocationId)
            .GreaterThan(0).WithMessage("Location ID must be greater than 0.");
    }
}