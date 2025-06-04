using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;
using Market.Domain.Entities.Market;

namespace Market.Application.Features.ProcurementDetails.Queries.GetProcurementDetailsByProcurement;

public class GetProcurementDetailsByProcurementHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetProcurementDetailsByProcurementQuery, List<ProcurementDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<List<ProcurementDetailDto>>> Handle(GetProcurementDetailsByProcurementQuery request, CancellationToken cancellationToken)
    {
        // First verify the procurement exists
        var procurement = await _unitOfWork.Procurements.GetByIdAsync(request.ProcurementId);
        if (procurement is null)
        {
            return BaseResponse<List<ProcurementDetailDto>>.Failure(["Procurement not found."]);
        }

        IEnumerable<ProcurementDetail> procurementDetails;

        if (request.IncludeProductDetails)
        {
            procurementDetails = await _unitOfWork.ProcurementDetails.GetProcurementDetailsWithProductsAsync(request.ProcurementId);
        }
        else
        {
            procurementDetails = await _unitOfWork.ProcurementDetails.GetByProcurementAsync(request.ProcurementId);
        }

        var procurementDetailDtos = _mapper.Map<List<ProcurementDetailDto>>(procurementDetails);

        return BaseResponse<List<ProcurementDetailDto>>.Success(procurementDetailDtos, $"Found {procurementDetailDtos.Count} procurement details for procurement.");
    }
}