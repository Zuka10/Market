using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Procurements.Commands.UpdateProcurement;

public class UpdateProcurementHandler(IUnitOfWork unitOfWork) : ICommandHandler<UpdateProcurementCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<BaseResponse<bool>> Handle(UpdateProcurementCommand request, CancellationToken cancellationToken)
    {
        var existingProcurement = await _unitOfWork.Procurements.GetByIdAsync(request.ProcurementId);
        if (existingProcurement is null)
        {
            return BaseResponse<bool>.Failure(["Procurement not found."]);
        }

        existingProcurement.VendorId = request.VendorId;
        existingProcurement.LocationId = request.LocationId;
        existingProcurement.ReferenceNo = request.ReferenceNo.Trim().ToUpperInvariant();
        existingProcurement.ProcurementDate = request.ProcurementDate;
        existingProcurement.TotalAmount = request.TotalAmount;
        existingProcurement.Notes = request.Notes?.Trim();
        existingProcurement.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Procurements.UpdateAsync(existingProcurement);
        return BaseResponse<bool>.Success(true, "Procurement updated successfully.");
    }
}