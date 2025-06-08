using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Application.Features.OrderDetails.Commands.DeleteOrderDetail;
using Market.Application.Features.OrderDetails.Commands.UpdateOrderDetail;
using Market.Application.Features.OrderDetails.Queries.GetOrderDetailById;
using Market.Application.Features.OrderDetails.Queries.GetOrderDetailsByOrder;
using Market.Application.Features.OrderDetails.Queries.GetOrderDetailsByProduct;
using Market.Domain.Filters;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Market.API.Controllers;

/// <summary>
/// Controller for managing order details.
/// </summary>
[Route("api/order-details")]
[ApiController]
[Authorize(Roles = "Admin")]
public class OrderDetailController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Retrieves all order details for a specific order
    /// </summary>
    /// <param name="id">The order ID</param>
    /// <param name="includeProductDetails">Include product details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of order details for the order</returns>
    [HttpGet("order/{id:int}")]
    public async Task<ActionResult<BaseResponse<PagedResult<OrderDetailDto>>>> GetOrderDetailsByOrder(
        int id,
        [FromQuery] bool includeProductDetails = true,
        CancellationToken cancellationToken = default)
    {
        var query = new GetOrderDetailsByOrderQuery(
            OrderId: id,
            IncludeProductDetails: includeProductDetails);

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves order details for a specific product across all orders
    /// </summary>
    /// <param name="id">The product ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of order details for the product</returns>
    [HttpGet("product/{id:int}")]
    public async Task<ActionResult<BaseResponse<PagedResult<OrderDetailDto>>>> GetOrderDetailsByProduct(
        int id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetOrderDetailsByProductQuery(ProductId: id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a specific order detail by its ID
    /// </summary>
    /// <param name="id">The order detail ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The order detail information</returns>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOrderDetailById(
        int id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetOrderDetailByIdQuery(Id: id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates an existing order detail
    /// </summary>
    /// <param name="id">The order detail ID to update</param>
    /// <param name="command">Order detail update details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated order detail</returns>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateOrderDetail(
        int id,
        [FromBody] UpdateOrderDetailCommand command,
        CancellationToken cancellationToken = default)
    {
        var updateCommand = command with { OrderDetailId = id };
        var result = await _mediator.Send(updateCommand, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Removes an order detail from an order
    /// </summary>
    /// <param name="id">The order detail ID to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deletion confirmation</returns>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteOrderDetail(
        int id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteOrderDetailCommand(OrderDetailId: id);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}