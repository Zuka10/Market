using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Discounts.Queries.GetDiscountsByLocation;

public class GetDiscountsByLocationHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetDiscountsByLocationQuery, List<DiscountDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<List<DiscountDto>>> Handle(GetDiscountsByLocationQuery request, CancellationToken cancellationToken)
    {
        var discounts = await _unitOfWork.Discounts.GetDiscountsByLocationAsync(request.LocationId);
        var discountDtos = _mapper.Map<List<DiscountDto>>(discounts);

        return BaseResponse<List<DiscountDto>>.Success(discountDtos, $"Found {discountDtos.Count} discounts for location.");
    }
}