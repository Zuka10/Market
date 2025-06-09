using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Application.Features.Payments.Commands.CancelPayment;
using Market.Application.Features.Payments.Commands.CreatePayment;
using Market.Application.Features.Payments.Commands.UpdatePayment;
using Market.Application.Features.Payments.Commands.UpdatePaymentStatus;
using Market.Application.Features.Payments.Queries.GetPaymentById;
using Market.Application.Features.Payments.Queries.GetPayments;
using Market.Application.Features.Payments.Queries.GetPaymentsByMethod;
using Market.Application.Features.Payments.Queries.GetPaymentsByOrder;
using Market.Application.Features.Payments.Queries.GetPaymentsByStatus;
using Market.Domain.Enums;
using Market.Domain.Filters;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Market.API.Controllers;

/// <summary>
/// Controller for managing payments.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class PaymentController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Retrieves a paginated and filtered list of payments
    /// </summary>
    /// <param name="query">Payment search filters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of payments</returns>
    [HttpGet]
    public async Task<ActionResult<BaseResponse<PagedResult<PaymentDto>>>> GetAll(
        [FromQuery] GetPaymentsQuery query,
        CancellationToken cancellationToken = default)
    {
        var payments = await _mediator.Send(query, cancellationToken);
        return Ok(payments);
    }

    /// <summary>
    /// Retrieves a specific payment by its ID
    /// </summary>
    /// <param name="id">The payment ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The payment details</returns>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetPaymentById(
        int id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPaymentByIdQuery(Id: id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new payment
    /// </summary>
    /// <param name="command">Payment creation details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created payment</returns>
    [HttpPost]
    public async Task<IActionResult> CreatePayment(
        [FromBody] CreatePaymentCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates an existing payment
    /// </summary>
    /// <param name="id">The payment ID to update</param>
    /// <param name="command">Payment update details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated payment</returns>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdatePayment(
        int id,
        [FromBody] UpdatePaymentCommand command,
        CancellationToken cancellationToken = default)
    {
        var updateCommand = command with { PaymentId = id };
        var result = await _mediator.Send(updateCommand, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Cancels a payment
    /// </summary>
    /// <param name="id">The payment ID to cancel</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cancellation confirmation</returns>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> CancelPayment(
        int id,
        CancellationToken cancellationToken = default)
    {
        var command = new CancelPaymentCommand(PaymentId: id);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves payments for a specific order
    /// </summary>
    /// <param name="id">The order ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of payments for the order</returns>
    [HttpGet("order/{id:int}")]
    public async Task<IActionResult> GetPaymentsByOrder(
        int id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPaymentsByOrderQuery(OrderId: id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves payments by payment method
    /// </summary>
    /// <param name="method">The payment method</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of payments with the specified method</returns>
    [HttpGet("method/{method}")]
    public async Task<IActionResult> GetPaymentsByMethod(
        PaymentMethod method,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPaymentsByMethodQuery(PaymentMethod: method);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves payments by status
    /// </summary>
    /// <param name="status">The payment status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of payments with the specified status</returns>
    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetPaymentsByStatus(
        PaymentStatus status,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPaymentsByStatusQuery(Status: status);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates the status of a payment
    /// </summary>
    /// <param name="id">The payment ID</param>
    /// <param name="command">Status update details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated payment with new status</returns>
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdatePaymentStatus(
        int id,
        [FromBody] UpdatePaymentStatusCommand command,
        CancellationToken cancellationToken = default)
    {
        var updateCommand = command with { PaymentId = id };
        var result = await _mediator.Send(updateCommand, cancellationToken);
        return Ok(result);
    }
}