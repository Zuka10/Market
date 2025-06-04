using Market.Application.Common.Interfaces;

namespace Market.Application.Features.VendorLocations.Commands.UpdateVendorLocation;

public record UpdateVendorLocationCommand(
    long Id,
    long VendorId,
    long LocationId,
    string? StallNumber,
    decimal RentAmount,
    DateTime StartDate,
    DateTime? EndDate,
    bool IsActive
) : ICommand<bool>;