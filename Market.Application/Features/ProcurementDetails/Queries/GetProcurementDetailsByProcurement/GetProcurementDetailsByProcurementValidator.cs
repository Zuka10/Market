using FluentValidation;

namespace Market.Application.Features.ProcurementDetails.Queries.GetProcurementDetailsByProcurement;

public class GetProcurementDetailsByProcurementValidator : AbstractValidator<GetProcurementDetailsByProcurementQuery>
{
    public GetProcurementDetailsByProcurementValidator()
    {
        RuleFor(x => x.ProcurementId)
            .GreaterThan(0).WithMessage("Procurement ID must be greater than 0.");
    }
}