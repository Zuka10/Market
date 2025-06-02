using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Vendors.Queries.GetVendorById;

public class GetVendorByIdHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetVendorByIdQuery, VendorDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<VendorDto>> Handle(GetVendorByIdQuery request, CancellationToken cancellationToken)
    {
        var vendor = await _unitOfWork.Vendors.GetByIdAsync(request.Id);
        if (vendor is null)
        {
            return BaseResponse<VendorDto>.Failure(["Vendor not found."]);
        }

        var vendorDto = _mapper.Map<VendorDto>(vendor);

        return BaseResponse<VendorDto>.Success(vendorDto, "Vendor retrieved successfully.");
    }
}