using FluentValidation;

namespace Market.Application.Features.Vendors.Queries.GetVendorByEmail;

public class GetVendorByEmailValidator : AbstractValidator<GetVendorByEmailQuery>
{
    public GetVendorByEmailValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email cannot be empty.")
            .EmailAddress()
            .WithMessage("Invalid email format.");
    }
}