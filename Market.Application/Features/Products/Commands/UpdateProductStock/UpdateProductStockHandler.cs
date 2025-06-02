using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Products.Commands.UpdateProductStock;

public class UpdateProductStockHandler(IUnitOfWork unitOfWork) : ICommandHandler<UpdateProductStockCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<BaseResponse<bool>> Handle(UpdateProductStockCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.Products.UpdateStockAsync(request.ProductId, request.NewStock);
        return BaseResponse<bool>.Success(true, "Product stock updated successfully.");
    }
}