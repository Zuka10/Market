using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;

namespace Market.Application.Features.VendorLocations.Queries.GetVendorsByLocation;

public class GetVendorsByLocationHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetVendorsByLocationQuery, List<VendorLocationDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<List<VendorLocationDto>>> Handle(GetVendorsByLocationQuery request, CancellationToken cancellationToken)
    {
        var result = await _unitOfWork.VendorLocations.GetByLocationAsync(request.LocationId);
        var vendorLocationDtos = _mapper.Map<List<VendorLocationDto>>(result);

        return BaseResponse<List<VendorLocationDto>>.Success(vendorLocationDtos, $"Found {vendorLocationDtos.Count} vendors for location.");
    }
}