using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;
using Market.Domain.Entities.Market;

namespace Market.Application.Features.OrderDetails.Queries.GetOrderDetailsByOrder;

public class GetOrderDetailsByOrderHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetOrderDetailsByOrderQuery, List<OrderDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<List<OrderDetailDto>>> Handle(GetOrderDetailsByOrderQuery request, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId);
        if (order is null)
        {
            return BaseResponse<List<OrderDetailDto>>.Failure(["Order not found."]);
        }

        IEnumerable<OrderDetail> orderDetails;

        if (request.IncludeProductDetails)
        {
            orderDetails = await _unitOfWork.OrderDetails.GetOrderDetailsWithProductsAsync(request.OrderId);
        }
        else
        {
            orderDetails = await _unitOfWork.OrderDetails.GetByOrderAsync(request.OrderId);
        }

        var orderDetailDtos = _mapper.Map<List<OrderDetailDto>>(orderDetails);

        return BaseResponse<List<OrderDetailDto>>.Success(orderDetailDtos, $"Found {orderDetailDtos.Count} order details for order.");
    }
}