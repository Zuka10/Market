using FluentValidation;

namespace Market.Application.Features.Categories.Command.UpdateCategory;

public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryValidator()
    {
        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category ID must be greater than 0.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(100).WithMessage("Category name cannot exceed 100 characters.")
            .Must(BeAValidCategoryName).WithMessage("Category name contains invalid characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }

    private static bool BeAValidCategoryName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        // Category name should not contain special characters except spaces, hyphens, and parentheses
        return System.Text.RegularExpressions.Regex.IsMatch(name, @"^[a-zA-Z0-9\s\-\(\)&]+$");
    }
}