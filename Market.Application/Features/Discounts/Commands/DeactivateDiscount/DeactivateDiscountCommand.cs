using Market.Application.Common.Interfaces;

namespace Market.Application.Features.Discounts.Commands.DeactivateDiscount;

public record DeactivateDiscountCommand(long DiscountId) : ICommand<bool>;