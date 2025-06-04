using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.Locations.Queries.GetLocationById;


public record GetLocationByIdQuery(long LocationId) : IQuery<LocationDto>;