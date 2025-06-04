using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;

namespace Market.Application.Features.VendorLocations.Queries.GetVendorLocations;

public class GetVendorLocationsHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetVendorLocationsQuery, List<VendorLocationDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<List<VendorLocationDto>>> Handle(GetVendorLocationsQuery request, CancellationToken cancellationToken)
    {
        var vendorLocations = await _unitOfWork.VendorLocations.GetVendorLocationsWithDetailsAsync();
        var vendorLocationDtos = _mapper.Map<List<VendorLocationDto>>(vendorLocations);

        return BaseResponse<List<VendorLocationDto>>.Success(vendorLocationDtos, "Vendor locations retrieved successfully.");
    }
}