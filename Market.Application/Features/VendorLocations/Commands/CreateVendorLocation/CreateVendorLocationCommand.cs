using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.VendorLocations.Commands.CreateVendorLocation;

public record CreateVendorLocationCommand(
    long VendorId,
    long LocationId,
    string? StallNumber,
    decimal RentAmount,
    DateTime StartDate,
    DateTime? EndDate,
    bool IsActive = true
) : ICommand<VendorLocationDto>;