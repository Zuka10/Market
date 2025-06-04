using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.Categories.Queries.GetCategoryById;

public record GetCategoryByIdQuery(long Id) : IQuery<CategoryDto>
{
}