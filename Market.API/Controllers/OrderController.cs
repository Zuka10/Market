using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Application.Features.Orders.Commands.ApplyDiscountToOrder;
using Market.Application.Features.Orders.Commands.CancelOrder;
using Market.Application.Features.Orders.Commands.CreateOrder;
using Market.Application.Features.Orders.Commands.UpdateOrder;
using Market.Application.Features.Orders.Commands.UpdateOrderStatus;
using Market.Application.Features.Orders.Queries.GetOrderById;
using Market.Application.Features.Orders.Queries.GetOrderByOrderNumber;
using Market.Application.Features.Orders.Queries.GetOrderByUser;
using Market.Application.Features.Orders.Queries.GetOrders;
using Market.Application.Features.Orders.Queries.GetOrdersByDateRange;
using Market.Application.Features.Orders.Queries.GetOrdersByLocation;
using Market.Application.Features.Orders.Queries.GetOrdersByStatus;
using Market.Domain.Enums;
using Market.Domain.Filters;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Market.API.Controllers;

/// <summary>
/// Controller for managing orders.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class OrderController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Retrieves a paginated and filtered list of orders
    /// </summary>
    /// <param name="query">Order search filters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of orders</returns>
    [HttpGet]
    public async Task<ActionResult<BaseResponse<PagedResult<OrderDto>>>> GetAll(
        [FromQuery] GetOrdersQuery query,
        CancellationToken cancellationToken = default)
    {
        var orders = await _mediator.Send(query, cancellationToken);
        return Ok(orders);
    }

    /// <summary>
    /// Retrieves a specific order by its ID
    /// </summary>
    /// <param name="id">The order ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The order details</returns>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOrderById(
        int id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetOrderByIdQuery(Id: id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new order
    /// </summary>
    /// <param name="command">Order creation details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created order</returns>
    [HttpPost]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreateOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates an existing order
    /// </summary>
    /// <param name="id">The order ID to update</param>
    /// <param name="command">Order update details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated order</returns>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateOrder(
        int id,
        [FromBody] UpdateOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        var updateCommand = command with { OrderId = id };
        var result = await _mediator.Send(updateCommand, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Cancels an order
    /// </summary>
    /// <param name="id">The order ID to cancel</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cancellation confirmation</returns>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> CancelOrder(
        int id,
        CancellationToken cancellationToken = default)
    {
        var command = new CancelOrderCommand(OrderId: id);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates the status of an order
    /// </summary>
    /// <param name="id">The order ID</param>
    /// <param name="command">Status update details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated order with new status</returns>
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateOrderStatus(
        int id,
        [FromBody] UpdateOrderStatusCommand command,
        CancellationToken cancellationToken = default)
    {
        var updateCommand = command with { OrderId = id };
        var result = await _mediator.Send(updateCommand, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Applies a discount to an order
    /// </summary>
    /// <param name="id">The order ID</param>
    /// <param name="command">Discount application details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated order with applied discount</returns>
    [HttpPatch("{id:int}/discount")]
    public async Task<IActionResult> ApplyDiscount(
        int id,
        [FromBody] ApplyDiscountToOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        var discountCommand = command with { OrderId = id };
        var result = await _mediator.Send(discountCommand, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves an order by its order number
    /// </summary>
    /// <param name="number">The order number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The order details</returns>
    [HttpGet("number/{number}")]
    public async Task<IActionResult> GetOrderByNumber(
        string number,
        CancellationToken cancellationToken = default)
    {
        var query = new GetOrderByNumberQuery(OrderNumber: number);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves orders for a specific user
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of orders for the user</returns>
    [HttpGet("user/{id:int}")]
    public async Task<IActionResult> GetOrdersByUser(
        int id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetOrdersByUserQuery(UserId: id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves orders for a specific location
    /// </summary>
    /// <param name="id">The location ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of orders for the location</returns>
    [HttpGet("location/{id:int}")]
    public async Task<IActionResult> GetOrdersByLocation(
        int id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetOrdersByLocationQuery(LocationId: id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves orders by status
    /// </summary>
    /// <param name="status">The order status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of orders with the specified status</returns>
    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetOrdersByStatus(
        OrderStatus status,
        CancellationToken cancellationToken = default)
    {
        var query = new GetOrdersByStatusQuery(Status: status);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves orders within a specified date range
    /// </summary>
    /// <param name="startDate">Start date for the range</param>
    /// <param name="endDate">End date for the range</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of orders within the date range</returns>
    [HttpGet("date-range")]
    public async Task<IActionResult> GetOrdersByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var query = new GetOrdersByDateRangeQuery(
            StartDate: startDate,
            EndDate: endDate);

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}