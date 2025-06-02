using FluentValidation;

namespace Market.Application.Features.Orders.Queries.GetOrders;

public class GetOrdersValidator : AbstractValidator<GetOrdersQuery>
{
    public GetOrdersValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100.");

        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User ID must be greater than 0.")
            .When(x => x.UserId.HasValue);

        RuleFor(x => x.LocationId)
            .GreaterThan(0).WithMessage("Location ID must be greater than 0.")
            .When(x => x.LocationId.HasValue);

        RuleFor(x => x.DiscountId)
            .GreaterThan(0).WithMessage("Discount ID must be greater than 0.")
            .When(x => x.DiscountId.HasValue);

        RuleFor(x => x.MinTotal)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum total cannot be negative.")
            .When(x => x.MinTotal.HasValue);

        RuleFor(x => x.MaxTotal)
            .GreaterThanOrEqualTo(0).WithMessage("Maximum total cannot be negative.")
            .When(x => x.MaxTotal.HasValue);

        RuleFor(x => x)
            .Must(x => !x.MinTotal.HasValue || !x.MaxTotal.HasValue || x.MinTotal <= x.MaxTotal)
            .WithMessage("Minimum total cannot be greater than maximum total.");

        RuleFor(x => x)
            .Must(x => !x.StartDate.HasValue || !x.EndDate.HasValue || x.StartDate <= x.EndDate)
            .WithMessage("Start date cannot be later than end date.");

        RuleFor(x => x.SortBy)
            .Must(BeAValidSortField).WithMessage("Invalid sort field.")
            .When(x => !string.IsNullOrEmpty(x.SortBy));

        RuleFor(x => x.SortDirection)
            .Must(BeAValidSortDirection).WithMessage("Sort direction must be 'asc' or 'desc'.")
            .When(x => !string.IsNullOrEmpty(x.SortDirection));
    }

    private static bool BeAValidSortField(string? sortBy)
    {
        if (string.IsNullOrEmpty(sortBy))
        {
            return true;
        }

        var validFields = new[] { "id", "ordernumber", "orderdate", "total", "subtotal", "status", "customername", "username", "locationname", "createdat" };
        return validFields.Contains(sortBy.ToLower());
    }

    private static bool BeAValidSortDirection(string? sortDirection)
    {
        if (string.IsNullOrEmpty(sortDirection))
        {
            return true;
        }

        var validDirections = new[] { "asc", "desc" };
        return validDirections.Contains(sortDirection.ToLower());
    }
}