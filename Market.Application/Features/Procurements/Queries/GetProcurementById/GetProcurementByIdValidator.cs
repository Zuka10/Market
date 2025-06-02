using FluentValidation;

namespace Market.Application.Features.Procurements.Queries.GetProcurementById;

public class GetProcurementByIdValidator : AbstractValidator<GetProcurementByIdQuery>
{
    public GetProcurementByIdValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Procurement ID must be greater than 0.");
    }
}