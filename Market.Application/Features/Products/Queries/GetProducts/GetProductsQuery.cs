using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;
using Market.Domain.Filters;

namespace Market.Application.Features.Products.Queries.GetProducts;

public record GetProductsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    bool? IsAvailable = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    int? MinStock = null,
    int? MaxStock = null,
    long? CategoryId = null,
    long? LocationId = null,
    string? Unit = null,
    string? SortBy = null,
    string? SortDirection = null
) : IQuery<PagedResult<ProductDto>>;