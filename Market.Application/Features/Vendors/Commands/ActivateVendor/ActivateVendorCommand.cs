using Market.Application.Common.Interfaces;

namespace Market.Application.Features.Vendors.Commands.ActivateVendor;

public record ActivateVendorCommand(long VendorId) : ICommand<bool>
{
}