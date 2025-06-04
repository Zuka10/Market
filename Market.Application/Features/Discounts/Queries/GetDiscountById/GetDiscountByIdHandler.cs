using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Discounts.Queries.GetDiscountById;

public class GetDiscountByIdHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetDiscountByIdQuery, DiscountDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<DiscountDto>> Handle(GetDiscountByIdQuery request, CancellationToken cancellationToken)
    {
        var discount = await _unitOfWork.Discounts.GetByIdAsync(request.Id);
        if (discount is null)
        {
            return BaseResponse<DiscountDto>.Failure(["Discount not found."]);
        }

        var discountDto = _mapper.Map<DiscountDto>(discount);
        return BaseResponse<DiscountDto>.Success(discountDto, "Discount retrieved successfully.");
    }
}