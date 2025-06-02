using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.VendorLocations.Queries.GetActiveVendorLocations;

public record GetActiveVendorLocationsQuery : IQuery<List<VendorLocationDto>>
{
}