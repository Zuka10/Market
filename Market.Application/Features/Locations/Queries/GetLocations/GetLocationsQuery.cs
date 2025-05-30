using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;
using Market.Domain.Filters;

namespace Market.Application.Features.Locations.Queries.GetLocations;

public record GetLocationsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    string? City = null,
    string? Country = null,
    bool? IsActive = null,
    string? SortBy = null,
    string? SortDirection = "asc"
) : IQuery<PagedResult<LocationDto>>;