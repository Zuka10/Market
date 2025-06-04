using FluentValidation;

namespace Market.Application.Features.Products.Queries.GetProducts;

public class GetProductsValidator : AbstractValidator<GetProductsQuery>
{
    public GetProductsValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100.");

        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum price cannot be negative.")
            .When(x => x.MinPrice.HasValue);

        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Maximum price cannot be negative.")
            .When(x => x.MaxPrice.HasValue);

        RuleFor(x => x)
            .Must(x => !x.MinPrice.HasValue || !x.MaxPrice.HasValue || x.MinPrice <= x.MaxPrice)
            .WithMessage("Minimum price cannot be greater than maximum price.");

        RuleFor(x => x.MinStock)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum stock cannot be negative.")
            .When(x => x.MinStock.HasValue);

        RuleFor(x => x.MaxStock)
            .GreaterThanOrEqualTo(0).WithMessage("Maximum stock cannot be negative.")
            .When(x => x.MaxStock.HasValue);

        RuleFor(x => x)
            .Must(x => !x.MinStock.HasValue || !x.MaxStock.HasValue || x.MinStock <= x.MaxStock)
            .WithMessage("Minimum stock cannot be greater than maximum stock.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category ID must be greater than 0.")
            .When(x => x.CategoryId.HasValue);

        RuleFor(x => x.LocationId)
            .GreaterThan(0).WithMessage("Location ID must be greater than 0.")
            .When(x => x.LocationId.HasValue);

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

        var validFields = new[] { "id", "name", "price", "instock", "unit", "isavailable", "categoryname", "locationname", "createdat", "updatedat" };
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