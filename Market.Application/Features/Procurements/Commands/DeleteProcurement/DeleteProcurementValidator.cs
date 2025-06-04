using FluentValidation;

namespace Market.Application.Features.Procurements.Commands.DeleteProcurement;

public class DeleteProcurementValidator : AbstractValidator<DeleteProcurementCommand>
{
    public DeleteProcurementValidator()
    {
        RuleFor(x => x.ProcurementId)
            .GreaterThan(0).WithMessage("Procurement ID must be greater than 0.");
    }
}