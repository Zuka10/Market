using FluentValidation;

namespace Market.Application.Features.Orders.Queries.GetOrdersByDateRange;

public class GetOrdersByDateRangeValidator : AbstractValidator<GetOrdersByDateRangeQuery>
{
    public GetOrdersByDateRangeValidator()
    {
        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required.");

        RuleFor(x => x)
            .Must(x => x.StartDate <= x.EndDate)
            .WithMessage("Start date cannot be later than end date.");
    }
}