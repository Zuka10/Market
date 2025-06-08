using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Application.Features.Procurements.Commands.CreateProcurement;
using Market.Application.Features.Procurements.Commands.DeleteProcurement;
using Market.Application.Features.Procurements.Commands.UpdateProcurement;
using Market.Application.Features.Procurements.Queries.GetProcurementById;
using Market.Application.Features.Procurements.Queries.GetProcurementByReference;
using Market.Application.Features.Procurements.Queries.GetProcurements;
using Market.Application.Features.Procurements.Queries.GetProcurementsByDateRange;
using Market.Application.Features.Procurements.Queries.GetProcurementsByLocation;
using Market.Application.Features.Procurements.Queries.GetProcurementsByVendor;
using Market.Domain.Filters;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Market.API.Controllers;

/// <summary>
/// Controller for managing procurements.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class ProcurementController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Retrieves a paginated and filtered list of procurements
    /// </summary>
    /// <param name="query">Procurement search filters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of procurements</returns>
    [HttpGet]
    public async Task<ActionResult<BaseResponse<PagedResult<ProcurementDto>>>> GetAll(
        [FromQuery] GetProcurementsQuery query,
        CancellationToken cancellationToken = default)
    {
        var procurements = await _mediator.Send(query, cancellationToken);
        return Ok(procurements);
    }

    /// <summary>
    /// Retrieves a specific procurement by its ID
    /// </summary>
    /// <param name="id">The procurement ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The procurement details</returns>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetProcurementById(
        int id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProcurementByIdQuery(Id: id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new procurement
    /// </summary>
    /// <param name="command">Procurement creation details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created procurement</returns>
    [HttpPost]
    public async Task<IActionResult> CreateProcurement(
        [FromBody] CreateProcurementCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates an existing procurement
    /// </summary>
    /// <param name="id">The procurement ID to update</param>
    /// <param name="command">Procurement update details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated procurement</returns>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProcurement(
        int id,
        [FromBody] UpdateProcurementCommand command,
        CancellationToken cancellationToken = default)
    {
        var updateCommand = command with { ProcurementId = id };
        var result = await _mediator.Send(updateCommand, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Deletes a procurement
    /// </summary>
    /// <param name="id">The procurement ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deletion confirmation</returns>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProcurement(
        int id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteProcurementCommand(ProcurementId: id);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a procurement by its reference number
    /// </summary>
    /// <param name="reference">The procurement reference number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The procurement details</returns>
    [HttpGet("reference/{reference}")]
    public async Task<IActionResult> GetProcurementByReference(
        string reference,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProcurementByReferenceQuery(ReferenceNo: reference);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves procurements for a specific vendor
    /// </summary>
    /// <param name="id">The vendor ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of procurements for the vendor</returns>
    [HttpGet("vendor/{id:int}")]
    public async Task<IActionResult> GetProcurementsByVendor(
        int id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProcurementsByVendorQuery(VendorId: id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves procurements for a specific location
    /// </summary>
    /// <param name="id">The location ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of procurements for the location</returns>
    [HttpGet("location/{id:int}")]
    public async Task<IActionResult> GetProcurementsByLocation(
        int id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProcurementsByLocationQuery(LocationId: id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves procurements within a specified date range
    /// </summary>
    /// <param name="startDate">Start date for the range</param>
    /// <param name="endDate">End date for the range</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of procurements within the date range</returns>
    [HttpGet("date-range")]
    public async Task<IActionResult> GetProcurementsByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProcurementsByDateRangeQuery(
            StartDate: startDate,
            EndDate: endDate);

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}