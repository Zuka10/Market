using Market.Application.Common.Interfaces;

namespace Market.Application.Features.Discounts.Commands.DeleteDiscount;

public record DeleteDiscountCommand(long DiscountId) : ICommand<bool>;