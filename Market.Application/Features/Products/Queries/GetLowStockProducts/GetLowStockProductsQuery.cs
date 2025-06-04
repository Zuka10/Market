using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.Products.Queries.GetLowStockProducts;

public record GetLowStockProductsQuery(
    int Threshold = 10,
    long? LocationId = null,
    long? CategoryId = null
) : IQuery<List<ProductDto>>;