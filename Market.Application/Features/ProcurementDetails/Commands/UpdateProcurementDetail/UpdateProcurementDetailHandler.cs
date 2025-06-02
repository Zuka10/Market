using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Domain.Abstractions;

namespace Market.Application.Features.ProcurementDetails.Commands.UpdateProcurementDetail;

public class UpdateProcurementDetailHandler(IUnitOfWork unitOfWork) : ICommandHandler<UpdateProcurementDetailCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<BaseResponse<bool>> Handle(UpdateProcurementDetailCommand request, CancellationToken cancellationToken)
    {
        var existingProcurementDetail = await _unitOfWork.ProcurementDetails.GetByIdAsync(request.ProcurementDetailId);
        if (existingProcurementDetail is null)
        {
            return BaseResponse<bool>.Failure(["Procurement detail not found."]);
        }

        // Verify the product exists
        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId);
        if (product is null)
        {
            return BaseResponse<bool>.Failure(["Product not found."]);
        }

        // Store original values for procurement total recalculation
        var originalLineTotal = existingProcurementDetail.LineTotal;

        // Update procurement detail properties
        existingProcurementDetail.ProductId = request.ProductId;
        existingProcurementDetail.PurchasePrice = request.PurchasePrice;
        existingProcurementDetail.Quantity = request.Quantity;
        existingProcurementDetail.LineTotal = request.LineTotal;

        await _unitOfWork.ProcurementDetails.UpdateAsync(existingProcurementDetail);

        // Update procurement totals if line total changed
        if (Math.Abs(originalLineTotal - request.LineTotal) > 0.01m)
        {
            await UpdateProcurementTotalsAsync(existingProcurementDetail.ProcurementId, request.LineTotal - originalLineTotal);
        }

        return BaseResponse<bool>.Success(true, "Procurement detail updated successfully.");
    }

    private async Task UpdateProcurementTotalsAsync(long procurementId, decimal lineTotalDifference)
    {
        var procurement = await _unitOfWork.Procurements.GetByIdAsync(procurementId);
        if (procurement is not null)
        {
            procurement.TotalAmount += lineTotalDifference;
            procurement.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Procurements.UpdateAsync(procurement);
        }
    }
}