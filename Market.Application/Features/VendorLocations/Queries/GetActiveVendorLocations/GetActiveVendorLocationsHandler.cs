using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;

namespace Market.Application.Features.VendorLocations.Queries.GetActiveVendorLocations;

public class GetActiveVendorLocationsHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetActiveVendorLocationsQuery, List<VendorLocationDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<List<VendorLocationDto>>> Handle(GetActiveVendorLocationsQuery request, CancellationToken cancellationToken)
    {
        var vendorLocations = await _unitOfWork.VendorLocations.GetActiveVendorLocationsAsync();
        if (vendorLocations is null || !vendorLocations.Any())
        {
            return BaseResponse<List<VendorLocationDto>>.Failure(["No active vendor locations found."]);
        }

        var vendorLocationDtos = _mapper.Map<List<VendorLocationDto>>(vendorLocations);
        return BaseResponse<List<VendorLocationDto>>.Success(vendorLocationDtos, "Active vendor locations retrieved successfully.");
    }
}