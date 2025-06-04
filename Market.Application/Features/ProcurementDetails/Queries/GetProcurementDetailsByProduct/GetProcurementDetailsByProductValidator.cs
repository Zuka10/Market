using FluentValidation;

namespace Market.Application.Features.ProcurementDetails.Queries.GetProcurementDetailsByProduct;

public class GetProcurementDetailsByProductValidator : AbstractValidator<GetProcurementDetailsByProductQuery>
{
    public GetProcurementDetailsByProductValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Product ID must be greater than 0.");
    }
}