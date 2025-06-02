using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.VendorLocations.Queries.GetLocationsByVendor;

public record GetLocationsByVendorQuery(long VendorId) : IQuery<List<VendorLocationDto>>;