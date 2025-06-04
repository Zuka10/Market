using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;

namespace Market.Application.Features.VendorLocations.Queries.GetVendorLocationById;

public class GetVendorLocationByIdHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetVendorLocationByIdQuery, VendorLocationDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<VendorLocationDto>> Handle(GetVendorLocationByIdQuery request, CancellationToken cancellationToken)
    {
        var vendorLocation = await _unitOfWork.VendorLocations.GetByIdAsync(request.Id);
        if (vendorLocation is null)
        {
            return BaseResponse<VendorLocationDto>.Failure(["Vendor-location relationship not found."]);
        }

        var vendorLocationDto = _mapper.Map<VendorLocationDto>(vendorLocation);
        return BaseResponse<VendorLocationDto>.Success(vendorLocationDto, "Vendor-location retrieved successfully.");
    }
}