using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.Locations.Queries.SearchLocations;

public record SearchLocationsQuery(string SearchTerm) : IQuery<List<LocationDto>>;