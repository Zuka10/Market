using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Application.Features.Discounts.Commands.ActivateDiscount;
using Market.Application.Features.Discounts.Commands.CreateDiscount;
using Market.Application.Features.Discounts.Commands.DeactivateDiscount;
using Market.Application.Features.Discounts.Commands.DeleteDiscount;
using Market.Application.Features.Discounts.Commands.UpdateDiscount;
using Market.Application.Features.Discounts.Queries.GetDiscountByCode;
using Market.Application.Features.Discounts.Queries.GetDiscountById;
using Market.Application.Features.Discounts.Queries.GetDiscounts;
using Market.Application.Features.Discounts.Queries.GetDiscountsByLocation;
using Market.Application.Features.Discounts.Queries.GetDiscountsByVendor;
using Market.Application.Features.Discounts.Queries.GetValidDiscounts;
using Market.Domain.Filters;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Market.API.Controllers;

/// <summary>
/// Controller for managing discounts.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,VendorManager")]
public class DiscountController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Retrieves a paginated and filtered list of discounts
    /// </summary>
    /// <param name="query">Discount search filters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of discounts</returns>
    [HttpGet]
    public async Task<ActionResult<BaseResponse<PagedResult<DiscountDto>>>> GetAll(
        [FromQuery] GetDiscountsQuery query,
        CancellationToken cancellationToken = default)
    {
        var discounts = await _mediator.Send(query, cancellationToken);
        return Ok(discounts);
    }

    /// <summary>
    /// Retrieves a specific discount by its ID
    /// </summary>
    /// <param name="id">The discount ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The discount details</returns>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetDiscountById(
        int id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDiscountByIdQuery(Id: id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new discount
    /// </summary>
    /// <param name="command">Discount creation details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created discount</returns>
    [HttpPost]
    public async Task<IActionResult> CreateDiscount(
        [FromBody] CreateDiscountCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates an existing discount
    /// </summary>
    /// <param name="id">The discount ID to update</param>
    /// <param name="command">Discount update details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated discount</returns>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateDiscount(
        int id,
        [FromBody] UpdateDiscountCommand command,
        CancellationToken cancellationToken = default)
    {
        var updateCommand = command with { DiscountId = id };
        var result = await _mediator.Send(updateCommand, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Deletes a discount
    /// </summary>
    /// <param name="id">The discount ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deletion confirmation</returns>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteDiscount(
        int id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteDiscountCommand(DiscountId: id);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Activates a discount
    /// </summary>
    /// <param name="id">The discount ID to activate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Activation confirmation</returns>
    [HttpPatch("{id:int}/activate")]
    public async Task<IActionResult> ActivateDiscount(
        int id,
        CancellationToken cancellationToken = default)
    {
        var command = new ActivateDiscountCommand(DiscountId: id);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Deactivates a discount
    /// </summary>
    /// <param name="id">The discount ID to deactivate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deactivation confirmation</returns>
    [HttpPatch("{id:int}/deactivate")]
    public async Task<IActionResult> DeactivateDiscount(
        int id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeactivateDiscountCommand(DiscountId: id);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a discount by its code
    /// </summary>
    /// <param name="code">The discount code</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The discount details</returns>
    [HttpGet("code/{code}")]
    public async Task<IActionResult> GetDiscountByCode(
        string code,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDiscountByCodeQuery(Code: code);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves currently valid discounts
    /// </summary>
    /// <param name="locationId">Filter by location ID</param>
    /// <param name="vendorId">Filter by vendor ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of currently valid discounts</returns>
    [HttpGet("valid")]
    public async Task<IActionResult> GetValidDiscounts(
        [FromQuery] int? locationId = null,
        [FromQuery] int? vendorId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetValidDiscountsQuery(
            LocationId: locationId,
            VendorId: vendorId);

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves discounts for a specific location
    /// </summary>
    /// <param name="id">The location ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of discounts for the location</returns>
    [HttpGet("location/{id:int}")]
    public async Task<IActionResult> GetDiscountsByLocation(
        int id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDiscountsByLocationQuery(LocationId: id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves discounts for a specific vendor
    /// </summary>
    /// <param name="id">The vendor ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of discounts for the vendor</returns>
    [HttpGet("vendor/{id:int}")]
    public async Task<IActionResult> GetDiscountsByVendor(
        int id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDiscountsByVendorQuery(VendorId: id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}