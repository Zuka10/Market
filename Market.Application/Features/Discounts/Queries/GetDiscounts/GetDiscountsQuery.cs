using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;
using Market.Domain.Filters;

namespace Market.Application.Features.Discounts.Queries.GetDiscounts;

public record GetDiscountsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    bool? IsActive = null,
    bool? IsValid = null,
    decimal? MinPercentage = null,
    decimal? MaxPercentage = null,
    long? LocationId = null,
    long? VendorId = null,
    DateTime? StartDateFrom = null,
    DateTime? StartDateTo = null,
    DateTime? EndDateFrom = null,
    DateTime? EndDateTo = null,
    string? SortBy = null,
    string? SortDirection = null
) : IQuery<PagedResult<DiscountDto>>;