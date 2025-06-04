using FluentValidation;

namespace Market.Application.Features.ProcurementDetails.Queries.GetProcurementDetailById;

public class GetProcurementDetailByIdValidator : AbstractValidator<GetProcurementDetailByIdQuery>
{
    public GetProcurementDetailByIdValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Procurement detail ID must be greater than 0.");
    }
}