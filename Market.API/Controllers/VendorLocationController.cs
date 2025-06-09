using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Application.Features.VendorLocations.Commands.CreateVendorLocation;
using Market.Application.Features.VendorLocations.Commands.DeleteVendorLocation;
using Market.Application.Features.VendorLocations.Commands.ExtendContract;
using Market.Application.Features.VendorLocations.Commands.TerminateContract;
using Market.Application.Features.VendorLocations.Commands.UpdateVendorLocation;
using Market.Application.Features.VendorLocations.Queries.GetLocationsByVendor;
using Market.Application.Features.VendorLocations.Queries.GetVendorLocationById;
using Market.Application.Features.VendorLocations.Queries.GetVendorLocations;
using Market.Application.Features.VendorLocations.Queries.GetVendorsByLocation;
using Market.Domain.Filters;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Market.API.Controllers;

/// <summary>
/// Controller for managing vendor-location relationships.
/// </summary>
[Route("api/vendor-locations")]
[ApiController]
[Authorize(Roles = "Admin,VendorManager,LocationManager")]
public class VendorLocationController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Retrieves all vendor-location relationships
    /// </summary>
    /// <param name="query">Vendor-location search filters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of vendor-location relationships</returns>
    [HttpGet]
    public async Task<ActionResult<BaseResponse<PagedResult<VendorLocationDto>>>> GetAll(
        [FromQuery] GetVendorLocationsQuery query,
        CancellationToken cancellationToken = default)
    {
        var vendorLocations = await _mediator.Send(query, cancellationToken);
        return Ok(vendorLocations);
    }

    /// <summary>
    /// Retrieves a specific vendor-location relationship by its ID
    /// </summary>
    /// <param name="id">The vendor-location relationship ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The vendor-location relationship details</returns>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetVendorLocationById(
        int id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetVendorLocationByIdQuery(Id: id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new vendor-location relationship
    /// </summary>
    /// <param name="command">Vendor-location relationship creation details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created vendor-location relationship</returns>
    [HttpPost]
    public async Task<IActionResult> CreateVendorLocation(
        [FromBody] CreateVendorLocationCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates an existing vendor-location relationship
    /// </summary>
    /// <param name="id">The vendor-location relationship ID to update</param>
    /// <param name="command">Vendor-location relationship update details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated vendor-location relationship</returns>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateVendorLocation(
        int id,
        [FromBody] UpdateVendorLocationCommand command,
        CancellationToken cancellationToken = default)
    {
        var updateCommand = command with { Id = id };
        var result = await _mediator.Send(updateCommand, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Removes a vendor from a location (deletes the relationship)
    /// </summary>
    /// <param name="id">The vendor-location relationship ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deletion confirmation</returns>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteVendorLocation(
        int id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteVendorLocationCommand(Id: id);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves all locations for a specific vendor
    /// </summary>
    /// <param name="id">The vendor ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of locations for the vendor</returns>
    [HttpGet("vendor/{id:int}")]
    public async Task<IActionResult> GetLocationsByVendor(
        int id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetLocationsByVendorQuery(VendorId: id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves all vendors for a specific location
    /// </summary>
    /// <param name="id">The location ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of vendors for the location</returns>
    [HttpGet("location/{id:int}")]
    public async Task<IActionResult> GetVendorsByLocation(
        int id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetVendorsByLocationQuery(LocationId: id);

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Extends the contract for a vendor-location relationship
    /// </summary>
    /// <param name="id">The vendor-location relationship ID</param>
    /// <param name="command">Contract extension details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated vendor-location relationship with extended contract</returns>
    [HttpPatch("{id:int}/extend")]
    public async Task<IActionResult> ExtendContract(
        int id,
        [FromBody] ExtendContractCommand command,
        CancellationToken cancellationToken = default)
    {
        var extendCommand = command with { Id = id };
        var result = await _mediator.Send(extendCommand, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Terminates the contract for a vendor-location relationship
    /// </summary>
    /// <param name="id">The vendor-location relationship ID</param>
    /// <param name="command">Contract termination details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated vendor-location relationship with terminated contract</returns>
    [HttpPatch("{id:int}/terminate")]
    public async Task<IActionResult> TerminateContract(
        int id,
        [FromBody] TerminateContractCommand command,
        CancellationToken cancellationToken = default)
    {
        var terminateCommand = command with { Id = id };
        var result = await _mediator.Send(terminateCommand, cancellationToken);
        return Ok(result);
    }
}