using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.Products.Queries.GetProductById;

public record GetProductByIdQuery(long Id) : IQuery<ProductDto>;