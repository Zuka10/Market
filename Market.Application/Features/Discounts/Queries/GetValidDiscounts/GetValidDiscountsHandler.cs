using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;
using Market.Domain.Filters;

namespace Market.Application.Features.Discounts.Queries.GetValidDiscounts;

public class GetValidDiscountsHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetValidDiscountsQuery, List<DiscountDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<List<DiscountDto>>> Handle(GetValidDiscountsQuery request, CancellationToken cancellationToken)
    {
        var filterParams = new DiscountFilterParameters
        {
            IsValid = true,
            LocationId = request.LocationId,
            VendorId = request.VendorId,
            SortBy = "percentage",
            SortDirection = "desc",
            PageSize = int.MaxValue
        };

        var discounts = await _unitOfWork.Discounts.GetDiscountsAsync(filterParams);
        var discountDtos = _mapper.Map<List<DiscountDto>>(discounts.Items);

        return BaseResponse<List<DiscountDto>>.Success(discountDtos, $"Found {discountDtos.Count} valid discounts.");
    }
}