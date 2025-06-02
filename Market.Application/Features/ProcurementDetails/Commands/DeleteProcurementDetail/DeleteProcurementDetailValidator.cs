using FluentValidation;

namespace Market.Application.Features.ProcurementDetails.Commands.DeleteProcurementDetail;

public class DeleteProcurementDetailValidator : AbstractValidator<DeleteProcurementDetailCommand>
{
    public DeleteProcurementDetailValidator()
    {
        RuleFor(x => x.ProcurementDetailId)
            .GreaterThan(0).WithMessage("Procurement detail ID must be greater than 0.");
    }
}