using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.Vendors.Commands.CreateVendor;

public record CreateVendorCommand(
    string Name,
    string? ContactPerson,
    string Email,
    string? Phone,
    string? Address,
    decimal CommisionRate,
    bool IsActive = true
) : ICommand<VendorDto>;