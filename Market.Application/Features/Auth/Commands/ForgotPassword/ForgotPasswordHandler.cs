using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordHandler(IUnitOfWork unitOfWork, ITokenService tokenService, IEmailService emailService)
    : ICommandHandler<ForgotPasswordCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IEmailService _emailService = emailService;

    public async Task<BaseResponse<bool>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        // Get user by email
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email.Trim().ToLower());

        // Always return success for security (don't reveal if email exists)
        if (user == null || !user.IsActive)
        {
            return BaseResponse<bool>.Success(true, "If the email exists, a password reset link has been sent.");
        }

        // Generate password reset JWT token (short-lived, 1 hour)
        var resetToken = _tokenService.GeneratePasswordResetToken(user.Id, user.Email, TimeSpan.FromHours(1));

        // Send reset email
        await _emailService.SendPasswordResetEmailAsync(user.Email, user.FirstName!, resetToken);

        return BaseResponse<bool>.Success(true, "If the email exists, a password reset link has been sent.");
    }
}