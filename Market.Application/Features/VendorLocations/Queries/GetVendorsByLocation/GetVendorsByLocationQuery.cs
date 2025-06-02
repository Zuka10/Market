using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.VendorLocations.Queries.GetVendorsByLocation;

public record GetVendorsByLocationQuery(long LocationId) : IQuery<List<VendorLocationDto>>;