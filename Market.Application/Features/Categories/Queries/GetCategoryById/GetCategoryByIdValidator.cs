using FluentValidation;

namespace Market.Application.Features.Categories.Queries.GetCategoryById;

public class GetCategoryByIdValidator : AbstractValidator<GetCategoryByIdQuery>
{
    public GetCategoryByIdValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Category ID must be greater than 0.");
    }
}