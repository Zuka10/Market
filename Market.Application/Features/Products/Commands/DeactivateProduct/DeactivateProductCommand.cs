using Market.Application.Common.Interfaces;

namespace Market.Application.Features.Products.Commands.DeactivateProduct;

public record DeactivateProductCommand(long ProductId) : ICommand<bool>;