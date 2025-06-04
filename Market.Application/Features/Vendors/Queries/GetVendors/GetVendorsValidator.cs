using FluentValidation;

namespace Market.Application.Features.Vendors.Queries.GetVendors;

public class GetAllVendorsValidator : AbstractValidator<GetVendorsQuery>
{
    public GetAllVendorsValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100.");

        RuleFor(x => x.SearchTerm)
            .MaximumLength(100).WithMessage("Search term cannot exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.SearchTerm));

        RuleFor(x => x.Email)
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters.")
            .EmailAddress().WithMessage("Email must be a valid email address.")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.ContactPersonName)
            .MaximumLength(100).WithMessage("Contact person name cannot exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.ContactPersonName));

        RuleFor(x => x.MinCommissionRate)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum commission rate cannot be negative.")
            .LessThanOrEqualTo(100).WithMessage("Minimum commission rate cannot exceed 100%.")
            .When(x => x.MinCommissionRate.HasValue);

        RuleFor(x => x.MaxCommissionRate)
            .GreaterThanOrEqualTo(0).WithMessage("Maximum commission rate cannot be negative.")
            .LessThanOrEqualTo(100).WithMessage("Maximum commission rate cannot exceed 100%.")
            .When(x => x.MaxCommissionRate.HasValue);

        RuleFor(x => x)
            .Must(x => !x.MinCommissionRate.HasValue || !x.MaxCommissionRate.HasValue || x.MinCommissionRate <= x.MaxCommissionRate)
            .WithMessage("Minimum commission rate cannot be greater than maximum commission rate.")
            .When(x => x.MinCommissionRate.HasValue && x.MaxCommissionRate.HasValue);

        RuleFor(x => x.LocationId)
            .GreaterThan(0).WithMessage("Location ID must be greater than 0.")
            .When(x => x.LocationId.HasValue);

        RuleFor(x => x.SortBy)
            .Must(BeAValidSortField).WithMessage("Sort by must be one of: id, name, email, contactpersonname, phonenumber, commissionrate, createdat, updatedat")
            .When(x => !string.IsNullOrEmpty(x.SortBy));

        RuleFor(x => x.SortDirection)
            .Must(BeAValidSortDirection).WithMessage("Sort direction must be 'asc' or 'desc'")
            .When(x => !string.IsNullOrEmpty(x.SortDirection));
    }

    private static bool BeAValidSortField(string? sortBy)
    {
        if (string.IsNullOrEmpty(sortBy))
        {
            return true;
        }

        var validSortFields = new[] { "id", "name", "email", "contactpersonname", "phonenumber", "commissionrate", "createdat", "updatedat" };
        return validSortFields.Contains(sortBy.ToLower());
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