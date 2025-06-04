using FluentValidation;

namespace Market.Application.Features.Discounts.Queries.GetDiscounts;

public class GetDiscountsValidator : AbstractValidator<GetDiscountsQuery>
{
    public GetDiscountsValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100.");

        RuleFor(x => x.MinPercentage)
            .GreaterThan(0).WithMessage("Minimum percentage must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("Minimum percentage cannot exceed 100.")
            .When(x => x.MinPercentage.HasValue);

        RuleFor(x => x.MaxPercentage)
            .GreaterThan(0).WithMessage("Maximum percentage must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("Maximum percentage cannot exceed 100.")
            .When(x => x.MaxPercentage.HasValue);

        RuleFor(x => x)
            .Must(x => !x.MinPercentage.HasValue || !x.MaxPercentage.HasValue || x.MinPercentage <= x.MaxPercentage)
            .WithMessage("Minimum percentage cannot be greater than maximum percentage.");

        RuleFor(x => x.LocationId)
            .GreaterThan(0).WithMessage("Location ID must be greater than 0.")
            .When(x => x.LocationId.HasValue);

        RuleFor(x => x.VendorId)
            .GreaterThan(0).WithMessage("Vendor ID must be greater than 0.")
            .When(x => x.VendorId.HasValue);

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

        var validFields = new[] { "id", "discountcode", "description", "percentage", "startdate", "enddate", "isactive", "locationname", "vendorname", "createdat" };
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