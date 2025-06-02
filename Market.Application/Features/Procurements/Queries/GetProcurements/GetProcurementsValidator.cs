using FluentValidation;

namespace Market.Application.Features.Procurements.Queries.GetProcurements;

public class GetProcurementsValidator : AbstractValidator<GetProcurementsQuery>
{
    public GetProcurementsValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100.");

        RuleFor(x => x.VendorId)
            .GreaterThan(0).WithMessage("Vendor ID must be greater than 0.")
            .When(x => x.VendorId.HasValue);

        RuleFor(x => x.LocationId)
            .GreaterThan(0).WithMessage("Location ID must be greater than 0.")
            .When(x => x.LocationId.HasValue);

        RuleFor(x => x.MinAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum amount cannot be negative.")
            .When(x => x.MinAmount.HasValue);

        RuleFor(x => x.MaxAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Maximum amount cannot be negative.")
            .When(x => x.MaxAmount.HasValue);

        RuleFor(x => x)
            .Must(x => !x.MinAmount.HasValue || !x.MaxAmount.HasValue || x.MinAmount <= x.MaxAmount)
            .WithMessage("Minimum amount cannot be greater than maximum amount.");

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

        var validFields = new[] { "id", "referenceno", "procurementdate", "totalamount", "vendorname", "locationname", "notes", "createdat" };
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