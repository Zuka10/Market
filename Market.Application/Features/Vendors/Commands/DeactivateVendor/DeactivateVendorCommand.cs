using Market.Application.Common.Interfaces;

namespace Market.Application.Features.Vendors.Commands.DeactivateVendor;

public record DeactivateVendorCommand(long VendorId) : ICommand<bool>
{
}