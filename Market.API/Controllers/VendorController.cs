using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Application.Features.Vendors.Commands.ActivateVendor;
using Market.Application.Features.Vendors.Commands.CreateVendor;
using Market.Application.Features.Vendors.Commands.DeactivateVendor;
using Market.Application.Features.Vendors.Commands.DeleteVendor;
using Market.Application.Features.Vendors.Commands.UpdateVendor;
using Market.Application.Features.Vendors.Queries.GetVendorByEmail;
using Market.Application.Features.Vendors.Queries.GetVendorById;
using Market.Application.Features.Vendors.Queries.GetVendors;
using Market.Domain.Filters;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Market.API.Controllers;

/// <summary>
/// Controller for managing vendors.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,VendorManager")]
public class VendorController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Retrieves a paginated and filtered list of vendors
    /// </summary>
    /// <param name="query">Vendor search filters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of vendors</returns>
    [HttpGet]
    public async Task<ActionResult<BaseResponse<PagedResult<VendorDto>>>> GetAll(
        [FromQuery] GetVendorsQuery query,
        CancellationToken cancellationToken = default)
    {
        var vendors = await _mediator.Send(query, cancellationToken);
        return Ok(vendors);
    }

    /// <summary>
    /// Retrieves a specific vendor by their ID
    /// </summary>
    /// <param name="id">The vendor ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The vendor details</returns>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetVendorById(
        int id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetVendorByIdQuery(Id: id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new vendor
    /// </summary>
    /// <param name="command">Vendor creation details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created vendor</returns>
    [HttpPost]
    public async Task<IActionResult> CreateVendor(
        [FromBody] CreateVendorCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates an existing vendor
    /// </summary>
    /// <param name="id">The vendor ID to update</param>
    /// <param name="command">Vendor update details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated vendor</returns>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateVendor(
        int id,
        [FromBody] UpdateVendorCommand command,
        CancellationToken cancellationToken = default)
    {
        var updateCommand = command with { VendorId = id };
        var result = await _mediator.Send(updateCommand, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Deletes a vendor
    /// </summary>
    /// <param name="id">The vendor ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deletion confirmation</returns>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteVendor(
        int id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteVendorCommand(VendorId: id);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Activates a vendor account
    /// </summary>
    /// <param name="id">The vendor ID to activate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Activation confirmation</returns>
    [HttpPatch("{id:int}/activate")]
    public async Task<IActionResult> ActivateVendor(
        int id,
        CancellationToken cancellationToken = default)
    {
        var command = new ActivateVendorCommand(VendorId: id);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Deactivates a vendor account
    /// </summary>
    /// <param name="id">The vendor ID to deactivate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deactivation confirmation</returns>
    [HttpPatch("{id:int}/deactivate")]
    public async Task<IActionResult> DeactivateVendor(
        int id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeactivateVendorCommand(VendorId: id);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a vendor by their email address
    /// </summary>
    /// <param name="email">The vendor's email address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The vendor details</returns>
    [HttpGet("email/{email}")]
    public async Task<IActionResult> GetVendorByEmail(
        string email,
        CancellationToken cancellationToken = default)
    {
        var query = new GetVendorByEmailQuery(Email: email);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}