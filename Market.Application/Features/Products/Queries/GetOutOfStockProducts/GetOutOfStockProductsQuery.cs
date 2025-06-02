using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.Products.Queries.GetOutOfStockProducts;

public record GetOutOfStockProductsQuery(
    long? LocationId = null,
    long? CategoryId = null
) : IQuery<List<ProductDto>>;