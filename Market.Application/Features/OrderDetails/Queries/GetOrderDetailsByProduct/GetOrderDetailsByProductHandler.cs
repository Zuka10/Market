using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;

namespace Market.Application.Features.OrderDetails.Queries.GetOrderDetailsByProduct;

public class GetOrderDetailsByProductHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetOrderDetailsByProductQuery, List<OrderDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<List<OrderDetailDto>>> Handle(GetOrderDetailsByProductQuery request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId);
        if (product is null)
        {
            return BaseResponse<List<OrderDetailDto>>.Failure(["Product not found."]);
        }

        var orderDetails = await _unitOfWork.OrderDetails.GetByProductAsync(request.ProductId);
        var orderDetailDtos = _mapper.Map<List<OrderDetailDto>>(orderDetails);

        return BaseResponse<List<OrderDetailDto>>.Success(orderDetailDtos, $"Found {orderDetailDtos.Count} order details for product.");
    }
}