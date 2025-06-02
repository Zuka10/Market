using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;

namespace Market.Application.Features.VendorLocations.Queries.GetLocationsByVendor;

public class GetLocationsByVendorHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetLocationsByVendorQuery, List<VendorLocationDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<List<VendorLocationDto>>> Handle(GetLocationsByVendorQuery request, CancellationToken cancellationToken)
    {
        var result = await _unitOfWork.VendorLocations.GetByVendorAsync(request.VendorId);
        var vendorLocationDtos = _mapper.Map<List<VendorLocationDto>>(result);

        return BaseResponse<List<VendorLocationDto>>.Success(vendorLocationDtos, $"Found {vendorLocationDtos.Count} locations for vendor.");
    }
}