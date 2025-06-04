using Market.Application.Common.Interfaces;

namespace Market.Application.Features.Vendors.Commands.DeleteVendor;

public record DeleteVendorCommand(long VendorId) : ICommand<bool>
{
}