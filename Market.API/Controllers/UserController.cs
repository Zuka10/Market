using Market.Application.Common.Models;
using Market.Application.DTOs.Auth;
using Market.Application.Features.Users.Commands.ActivateUser;
using Market.Application.Features.Users.Commands.DeactivateUser;
using Market.Application.Features.Users.Commands.DeleteUser;
using Market.Application.Features.Users.Commands.UpdateUser;
using Market.Application.Features.Users.Queries.GetUserById;
using Market.Application.Features.Users.Queries.GetUsers;
using Market.Application.Features.Users.Queries.GetUsersByRole;
using Market.Domain.Filters;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Market.API.Controllers;

/// <summary>
/// Controller for managing users.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class UserController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Retrieves a paginated and filtered list of users
    /// </summary>
    /// <param name="query">User search filters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of users</returns>
    [HttpGet]
    public async Task<ActionResult<BaseResponse<PagedResult<UserDto>>>> GetAll(
        [FromQuery] GetUsersQuery query,
        CancellationToken cancellationToken = default)
    {
        var users = await _mediator.Send(query, cancellationToken);
        return Ok(users);
    }

    /// <summary>
    /// Retrieves a specific user by their ID
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The user details</returns>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetUserById(
        int id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUserByIdQuery(UserId: id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates an existing user
    /// </summary>
    /// <param name="id">The user ID to update</param>
    /// <param name="command">User update details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated user</returns>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateUser(
        int id,
        [FromBody] UpdateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var updateCommand = command with { UserId = id };
        var result = await _mediator.Send(updateCommand, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Deletes or deactivates a user
    /// </summary>
    /// <param name="id">The user ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deletion confirmation</returns>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUser(
        int id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteUserCommand(UserId: id);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Activates a user account
    /// </summary>
    /// <param name="id">The user ID to activate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Activation confirmation</returns>
    [HttpPatch("{id:int}/activate")]
    public async Task<IActionResult> ActivateUser(
        int id,
        CancellationToken cancellationToken = default)
    {
        var command = new ActivateUserCommand(UserId: id);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Deactivates a user account
    /// </summary>
    /// <param name="id">The user ID to deactivate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deactivation confirmation</returns>
    [HttpPatch("{id:int}/deactivate")]
    public async Task<IActionResult> DeactivateUser(
        int id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeactivateUserCommand(UserId: id);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves users by role
    /// </summary>
    /// <param name="roleId">The role ID</param>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of users with the specified role</returns>
    [HttpGet("roles/{roleId:int}")]
    public async Task<IActionResult> GetUsersByRole(
        int roleId,
        [FromQuery] bool isActive = true,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUsersByRoleQuery(
            RoleId: roleId,
            IsActive: isActive);

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}