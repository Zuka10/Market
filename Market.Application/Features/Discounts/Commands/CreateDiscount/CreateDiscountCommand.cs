using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.Discounts.Commands.CreateDiscount;

public record CreateDiscountCommand(
    string DiscountCode,
    string? Description,
    decimal Percentage,
    DateTime? StartDate,
    DateTime? EndDate,
    bool IsActive,
    long? LocationId,
    long? VendorId
) : ICommand<DiscountDto>;