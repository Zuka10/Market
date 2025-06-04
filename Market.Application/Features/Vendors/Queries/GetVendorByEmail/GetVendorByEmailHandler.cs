using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Vendors.Queries.GetVendorByEmail;

public class GetVendorByEmailHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetVendorByEmailQuery, VendorDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<VendorDto>> Handle(GetVendorByEmailQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BaseResponse<VendorDto>.Failure(["Email cannot be null or empty."]);
        }

        var vendor = await _unitOfWork.Vendors.GetByEmailAsync(request.Email.Trim());
        if (vendor is null)
        {
            return BaseResponse<VendorDto>.Failure(["Vendor not found."]);
        }

        var vendorDto = _mapper.Map<VendorDto>(vendor);

        return BaseResponse<VendorDto>.Success(vendorDto, "Vendor retrieved successfully.");
    }
}