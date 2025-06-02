using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Domain.Abstractions;

namespace Market.Application.Features.ProcurementDetails.Commands.DeleteProcurementDetail;

public class DeleteProcurementDetailHandler(IUnitOfWork unitOfWork) : ICommandHandler<DeleteProcurementDetailCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<BaseResponse<bool>> Handle(DeleteProcurementDetailCommand request, CancellationToken cancellationToken)
    {
        var existingProcurementDetail = await _unitOfWork.ProcurementDetails.GetByIdAsync(request.ProcurementDetailId);
        if (existingProcurementDetail is null)
        {
            return BaseResponse<bool>.Failure(["Procurement detail not found."]);
        }

        // Check if this is the last item in the procurement
        var procurementDetails = await _unitOfWork.ProcurementDetails.GetByProcurementAsync(existingProcurementDetail.ProcurementId);
        if (procurementDetails.Count() <= 1)
        {
            return BaseResponse<bool>.Failure(["Cannot remove the last item from a procurement. Cancel the entire procurement instead."]);
        }

        // Store values for procurement total recalculation
        var lineTotal = existingProcurementDetail.LineTotal;
        var procurementId = existingProcurementDetail.ProcurementId;

        await _unitOfWork.ProcurementDetails.DeleteAsync(request.ProcurementDetailId);

        // Update procurement totals
        await UpdateProcurementTotalsAsync(procurementId, -lineTotal);

        return BaseResponse<bool>.Success(true, "Procurement detail removed successfully.");
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