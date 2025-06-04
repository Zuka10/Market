using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.Products.Queries.GetProductsByCategory;

public record GetProductsByCategoryQuery(
    long CategoryId,
    bool? IsAvailable = true
) : IQuery<List<ProductDto>>;