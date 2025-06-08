using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Application.Features.Locations.Commands.ActivateLocation;
using Market.Application.Features.Locations.Commands.CreateLocation;
using Market.Application.Features.Locations.Commands.DeactivateLocation;
using Market.Application.Features.Locations.Commands.DeleteLocation;
using Market.Application.Features.Locations.Commands.UpdateLocation;
using Market.Application.Features.Locations.Queries.GetLocationById;
using Market.Application.Features.Locations.Queries.GetLocations;
using Market.Domain.Filters;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Market.API.Controllers;

/// <summary>
/// Controller for managing locations.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,LocationManager")]
public class LocationController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Retrieves a paginated and filtered list of locations
    /// </summary>
    /// <param name="query">Location search filters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of locations</returns>
    [HttpGet]
    public async Task<ActionResult<BaseResponse<PagedResult<LocationDto>>>> GetAll(
        [FromQuery] GetLocationsQuery query,
        CancellationToken cancellationToken = default)
    {
        var locations = await _mediator.Send(query, cancellationToken);
        return Ok(locations);
    }

    /// <summary>
    /// Retrieves a specific location by its ID
    /// </summary>
    /// <param name="id">The location ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The location details</returns>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetLocationById(
        int id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetLocationByIdQuery(LocationId: id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new location
    /// </summary>
    /// <param name="command">Location creation details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created location</returns>
    [HttpPost]
    public async Task<IActionResult> CreateLocation(
        [FromBody] CreateLocationCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates an existing location
    /// </summary>
    /// <param name="id">The location ID to update</param>
    /// <param name="command">Location update details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated location</returns>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateLocation(
        int id,
        [FromBody] UpdateLocationCommand command,
        CancellationToken cancellationToken = default)
    {
        var updateCommand = command with { LocationId = id };
        var result = await _mediator.Send(updateCommand, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Deletes a location
    /// </summary>
    /// <param name="id">The location ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deletion confirmation</returns>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteLocation(
        int id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteLocationCommand(LocationId: id);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Activates a location
    /// </summary>
    /// <param name="id">The location ID to activate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Activation confirmation</returns>
    [HttpPatch("{id:int}/activate")]
    public async Task<IActionResult> ActivateLocation(
        int id,
        CancellationToken cancellationToken = default)
    {
        var command = new ActivateLocationCommand(LocationId: id);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Deactivates a location
    /// </summary>
    /// <param name="id">The location ID to deactivate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deactivation confirmation</returns>
    [HttpPatch("{id:int}/deactivate")]
    public async Task<IActionResult> DeactivateLocation(
        int id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeactivateLocationCommand(LocationId: id);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}