using Market.Application.Common.Models;
using Market.Application.DTOs.Auth;
using Market.Application.Features.Roles.Commands.CreateRole;
using Market.Application.Features.Roles.Commands.DeleteRole;
using Market.Application.Features.Roles.Commands.UpdateRole;
using Market.Application.Features.Roles.Queries.GetRoleById;
using Market.Application.Features.Roles.Queries.GetRoles;
using Market.Application.Features.Roles.Queries.GetUsersInRole;
using Market.Domain.Filters;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Market.API.Controllers;

/// <summary>
/// Controller for managing roles.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class RoleController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Retrieves all roles
    /// </summary>
    /// <param name="query">Role search filters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of roles</returns>
    [HttpGet]
    public async Task<ActionResult<BaseResponse<PagedResult<RoleDto>>>> GetAll(
        [FromQuery] GetRolesQuery query,
        CancellationToken cancellationToken = default)
    {
        var roles = await _mediator.Send(query, cancellationToken);
        return Ok(roles);
    }

    /// <summary>
    /// Retrieves a specific role by its ID
    /// </summary>
    /// <param name="id">The role ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The role details</returns>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetRoleById(
        int id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetRoleByIdQuery(RoleId: id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new role
    /// </summary>
    /// <param name="command">Role creation details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created role</returns>
    [HttpPost]
    public async Task<IActionResult> CreateRole(
        [FromBody] CreateRoleCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates an existing role
    /// </summary>
    /// <param name="id">The role ID to update</param>
    /// <param name="command">Role update details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated role</returns>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateRole(
        int id,
        [FromBody] UpdateRoleCommand command,
        CancellationToken cancellationToken = default)
    {
        var updateCommand = command with { RoleId = id };
        var result = await _mediator.Send(updateCommand, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Deletes a role
    /// </summary>
    /// <param name="id">The role ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deletion confirmation</returns>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteRole(
        int id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteRoleCommand(RoleId: id);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves users assigned to a specific role
    /// </summary>
    /// <param name="id">The role ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of users in the specified role</returns>
    [HttpGet("{id:int}/users")]
    public async Task<IActionResult> GetUsersInRole(
        int id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUsersInRoleQuery(RoleId: id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}