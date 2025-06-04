using Market.Application.Common.Interfaces;

namespace Market.Application.Features.Discounts.Commands.ActivateDiscount;

public record ActivateDiscountCommand(long DiscountId) : ICommand<bool>;