using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.Vendors.Queries.GetVendorById;

public record GetVendorByIdQuery(long Id) : IQuery<VendorDto>
{
}