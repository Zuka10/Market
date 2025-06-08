using Market.Application.Features.Auth.Commands.ChangePassword;
using Market.Application.Features.Auth.Commands.ForgotPassword;
using Market.Application.Features.Auth.Commands.LoginUser;
using Market.Application.Features.Auth.Commands.LogoutUser;
using Market.Application.Features.Auth.Commands.RefreshToken;
using Market.Application.Features.Auth.Commands.RegisterUser;
using Market.Application.Features.Auth.Commands.ResetPassword;
using Market.Application.Features.Auth.Commands.RevokeTokens;
using Market.Application.Features.Auth.Queries.GetCurrentUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Market.API.Controllers;

/// <summary>
/// Controller for managing authentication and authorization.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class AuthController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Registers a new user account
    /// </summary>
    /// <param name="command">User registration details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Registration confirmation with user details</returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Authenticates a user and returns access and refresh tokens
    /// </summary>
    /// <param name="command">User login credentials</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication tokens and user information</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Refreshes an access token using a valid refresh token
    /// </summary>
    /// <param name="command">Refresh token details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>New access and refresh tokens</returns>
    [HttpPost("refresh")]
    [Authorize]
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Logs out a user by invalidating their tokens
    /// </summary>
    /// <param name="command">Logout details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Logout confirmation</returns>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Changes the password for the authenticated user
    /// </summary>
    /// <param name="command">Password change details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Password change confirmation</returns>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Initiates a password reset process by sending a reset token to the user's email
    /// </summary>
    /// <param name="command">Forgot password request details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Confirmation that reset instructions have been sent</returns>
    [HttpPost("forgot-password")]
    [Authorize]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Resets a user's password using a valid reset token
    /// </summary>
    /// <param name="command">Password reset details including token and new password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Password reset confirmation</returns>
    [HttpPost("reset-password")]
    [Authorize]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves information about the currently authenticated user
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current user information</returns>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken = default)
    {
        // Extract user ID from JWT claims
        var userIdClaim = User.FindFirst("userId") ?? User.FindFirst("sub");
        if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized();
        }

        var query = new GetCurrentUserQuery(UserId: userId);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Revokes all tokens for the authenticated user (forces re-authentication)
    /// </summary>
    /// <param name="command">Token revocation details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Confirmation that all tokens have been revoked</returns>
    [HttpPost("revoke-tokens")]
    [Authorize]
    public async Task<IActionResult> RevokeTokens(
        [FromBody] RevokeTokensCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}