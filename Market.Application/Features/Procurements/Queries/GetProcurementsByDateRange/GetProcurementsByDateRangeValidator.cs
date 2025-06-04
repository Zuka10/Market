using FluentValidation;

namespace Market.Application.Features.Procurements.Queries.GetProcurementsByDateRange;

internal class GetProcurementsByDateRangeValidator : AbstractValidator<GetProcurementsByDateRangeQuery>
{
    public GetProcurementsByDateRangeValidator()
    {
        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.")
            .LessThanOrEqualTo(x => x.EndDate).WithMessage("Start date must be before or equal to end date.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required.")
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("End date must be after or equal to start date.");

        RuleFor(x => x)
            .Must(x => x.StartDate <= x.EndDate)
            .WithMessage("Start date cannot be later than end date.");
    }
}