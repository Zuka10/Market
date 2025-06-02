using Market.Application.Common.Interfaces;

namespace Market.Application.Features.Discounts.Commands.UpdateDiscount;

public record UpdateDiscountCommand(
    long DiscountId,
    string DiscountCode,
    string? Description,
    decimal Percentage,
    DateTime? StartDate,
    DateTime? EndDate,
    bool IsActive,
    long? LocationId,
    long? VendorId
) : ICommand<bool>;