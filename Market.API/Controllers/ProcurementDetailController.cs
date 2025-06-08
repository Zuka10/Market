using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Application.Features.ProcurementDetails.Commands.DeleteProcurementDetail;
using Market.Application.Features.ProcurementDetails.Commands.UpdateProcurementDetail;
using Market.Application.Features.ProcurementDetails.Queries.GetProcurementDetailById;
using Market.Application.Features.ProcurementDetails.Queries.GetProcurementDetailsByProcurement;
using Market.Application.Features.ProcurementDetails.Queries.GetProcurementDetailsByProduct;
using Market.Domain.Filters;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Market.API.Controllers;

/// <summary>
/// Controller for managing procurement details.
/// </summary>
[Route("api/procurement-details")]
[ApiController]
[Authorize(Roles = "Admin")]
public class ProcurementDetailController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Retrieves all procurement details for a specific procurement
    /// </summary>
    /// <param name="id">The procurement ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of procurement details for the procurement</returns>
    [HttpGet("procurement/{id:int}")]
    public async Task<ActionResult<BaseResponse<PagedResult<ProcurementDetailDto>>>> GetProcurementDetailsByProcurement(
        int id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProcurementDetailsByProcurementQuery(ProcurementId: id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves procurement history for a specific product across all procurements
    /// </summary>
    /// <param name="id">The product ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of procurement details for the product</returns>
    [HttpGet("product/{id:int}")]
    public async Task<ActionResult<BaseResponse<PagedResult<ProcurementDetailDto>>>> GetProcurementDetailsByProduct(
        int id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProcurementDetailsByProductQuery(ProductId: id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a specific procurement detail by its ID
    /// </summary>
    /// <param name="id">The procurement detail ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The procurement detail information</returns>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetProcurementDetailById(
        int id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProcurementDetailByIdQuery(Id: id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates an existing procurement detail
    /// </summary>
    /// <param name="id">The procurement detail ID to update</param>
    /// <param name="command">Procurement detail update details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated procurement detail</returns>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProcurementDetail(
        int id,
        [FromBody] UpdateProcurementDetailCommand command,
        CancellationToken cancellationToken = default)
    {
        var updateCommand = command with { ProcurementDetailId = id };
        var result = await _mediator.Send(updateCommand, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Removes a procurement detail from a procurement
    /// </summary>
    /// <param name="id">The procurement detail ID to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deletion confirmation</returns>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProcurementDetail(
        int id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteProcurementDetailCommand(ProcurementDetailId: id);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}