using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;
using Market.Domain.Entities.Market;

namespace Market.Application.Features.Orders.Commands.CreateOrder;

public class CreateOrderHandler(IUnitOfWork unitOfWork, IMapper mapper) : ICommandHandler<CreateOrderCommand, OrderDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<OrderDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = new Order
        {
            OrderNumber = request.OrderNumber.Trim(),
            OrderDate = request.OrderDate,
            Total = request.Total,
            SubTotal = request.SubTotal,
            TotalCommission = request.TotalCommission,
            Status = request.Status,
            LocationId = request.LocationId,
            DiscountId = request.DiscountId,
            DiscountAmount = request.DiscountAmount,
            UserId = request.UserId,
            CustomerName = request.CustomerName?.Trim(),
            CustomerPhone = request.CustomerPhone?.Trim(),
            Notes = request.Notes?.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdOrder = await _unitOfWork.Orders.AddAsync(order);

        // Add order details
        foreach (var detail in request.OrderDetails)
        {
            var orderDetail = new OrderDetail
            {
                OrderId = createdOrder.Id,
                ProductId = detail.ProductId,
                Quantity = detail.Quantity,
                UnitPrice = detail.UnitPrice,
                LineTotal = detail.LineTotal,
                CostPrice = detail.CostPrice,
                Profit = detail.Profit
            };
            await _unitOfWork.OrderDetails.AddAsync(orderDetail);
        }

        var orderDto = _mapper.Map<OrderDto>(createdOrder);
        return BaseResponse<OrderDto>.Success(orderDto, "Order created successfully.");
    }
}