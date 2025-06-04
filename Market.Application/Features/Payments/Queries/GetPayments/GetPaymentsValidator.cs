using FluentValidation;

namespace Market.Application.Features.Payments.Queries.GetPayments;

public class GetPaymentsValidator : AbstractValidator<GetPaymentsQuery>
{
    public GetPaymentsValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100.");

        RuleFor(x => x)
            .Must(x => !x.StartDate.HasValue || !x.EndDate.HasValue || x.StartDate <= x.EndDate)
            .WithMessage("Start date cannot be later than end date.");

        RuleFor(x => x.SortBy)
            .Must(x => string.IsNullOrEmpty(x) || new[] { "Id", "PaymentDate", "Amount", "PaymentMethod", "Status", "OrderNumber", "OrderDate", "OrderTotal", "CustomerName", "Username", "LocationName", "CreatedAt", "UpdatedAt" }.Contains(x))
            .WithMessage("Invalid sort field.");

        RuleFor(x => x.SortDirection)
            .Must(x => string.IsNullOrEmpty(x) || new[] { "ASC", "DESC" }.Contains(x.ToUpper()))
            .WithMessage("Sort direction must be ASC or DESC.");
    }
}