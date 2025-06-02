using Market.Application.Common.Interfaces;

namespace Market.Application.Features.Vendors.Commands.UpdateVendor;

public record UpdateVendorCommand(
    int VendorId,
    string Name,
    string? ContactPerson,
    string Email,
    string Address,
    string? Phone,
    string? Description,
    decimal CommisionRate,
    bool IsActive
) : ICommand<bool>;