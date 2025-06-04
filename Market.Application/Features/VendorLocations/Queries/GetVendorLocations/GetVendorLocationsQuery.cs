using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.VendorLocations.Queries.GetVendorLocations;

public record GetVendorLocationsQuery : IQuery<List<VendorLocationDto>>;