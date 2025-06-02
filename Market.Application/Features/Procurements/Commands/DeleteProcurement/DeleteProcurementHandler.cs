using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Procurements.Commands.DeleteProcurement;

public class DeleteProcurementHandler(IUnitOfWork unitOfWork) : ICommandHandler<DeleteProcurementCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<BaseResponse<bool>> Handle(DeleteProcurementCommand request, CancellationToken cancellationToken)
    {
        var existingProcurement = await _unitOfWork.Procurements.GetByIdAsync(request.ProcurementId);
        if (existingProcurement is null)
        {
            return BaseResponse<bool>.Failure(["Procurement not found."]);
        }

        await _unitOfWork.Procurements.DeleteAsync(request.ProcurementId);
        return BaseResponse<bool>.Success(true, "Procurement deleted successfully.");
    }
}