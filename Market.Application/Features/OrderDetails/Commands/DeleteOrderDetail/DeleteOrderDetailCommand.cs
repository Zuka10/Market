using Market.Application.Common.Interfaces;

namespace Market.Application.Features.OrderDetails.Commands.DeleteOrderDetail;

public record DeleteOrderDetailCommand(long OrderDetailId) : ICommand<bool>;