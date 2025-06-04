using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.Products.Queries.GetProductsByLocation;

public record GetProductsByLocationQuery(
    long LocationId,
    bool? IsAvailable = true
) : IQuery<List<ProductDto>>;