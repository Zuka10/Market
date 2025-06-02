using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.Categories.Queries.SearchCategories;

public record SearchCategoriesQuery(string SearchTerm) : IQuery<List<CategoryDto>>
{
}