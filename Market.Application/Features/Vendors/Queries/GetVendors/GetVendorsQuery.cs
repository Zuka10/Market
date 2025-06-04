using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;
using Market.Domain.Filters;

namespace Market.Application.Features.Vendors.Queries.GetVendors;

public record GetVendorsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    string? Email = null,
    string? ContactPersonName = null,
    bool? IsActive = null,
    decimal? MinCommissionRate = null,
    decimal? MaxCommissionRate = null,
    long? LocationId = null,
    string? SortBy = null,
    string? SortDirection = null
) : IQuery<PagedResult<VendorDto>>;