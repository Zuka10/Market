using FluentValidation;

namespace Market.Application.Features.Auth.Commands.RevokeTokens;

public class RevokeTokensValidator : AbstractValidator<RevokeTokensCommand>
{
    public RevokeTokensValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("User ID is required.");
    }
}