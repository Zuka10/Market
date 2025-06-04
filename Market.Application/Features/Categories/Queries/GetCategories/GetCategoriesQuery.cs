using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.Categories.Queries.GetCategories;

public record GetCategoriesQuery : IQuery<List<CategoryDto>>
{
}