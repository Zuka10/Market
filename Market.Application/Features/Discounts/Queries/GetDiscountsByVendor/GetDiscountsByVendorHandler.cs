using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;
using Market.Domain.Filters;

namespace Market.Application.Features.Discounts.Queries.GetDiscountsByVendor;

public class GetDiscountsByVendorHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetDiscountsByVendorQuery, List<DiscountDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<List<DiscountDto>>> Handle(GetDiscountsByVendorQuery request, CancellationToken cancellationToken)
    {
        var discounts = await _unitOfWork.Discounts.GetDiscountsByVendorAsync(request.VendorId);
        var discountDtos = _mapper.Map<List<DiscountDto>>(discounts);

        return BaseResponse<List<DiscountDto>>.Success(discountDtos, $"Found {discountDtos.Count} discounts for vendor.");
    }
}