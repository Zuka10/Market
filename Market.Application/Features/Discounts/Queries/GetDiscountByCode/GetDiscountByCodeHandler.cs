using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Discounts.Queries.GetDiscountByCode;

public class GetDiscountByCodeHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetDiscountByCodeQuery, DiscountDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<DiscountDto>> Handle(GetDiscountByCodeQuery request, CancellationToken cancellationToken)
    {
        var discount = await _unitOfWork.Discounts.GetByCodeAsync(request.Code.Trim().ToUpperInvariant());
        if (discount is null)
        {
            return BaseResponse<DiscountDto>.Failure(["Discount with this code not found."]);
        }

        var discountDto = _mapper.Map<DiscountDto>(discount);
        return BaseResponse<DiscountDto>.Success(discountDto, "Discount retrieved successfully.");
    }
}