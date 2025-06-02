using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Procurements.Queries.GetProcurementByReference;

public class GetProcurementByReferenceHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetProcurementByReferenceQuery, ProcurementDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<ProcurementDto>> Handle(GetProcurementByReferenceQuery request, CancellationToken cancellationToken)
    {
        var procurement = await _unitOfWork.Procurements.GetByReferenceNoAsync(request.ReferenceNo.Trim().ToUpperInvariant());
        if (procurement is null)
        {
            return BaseResponse<ProcurementDto>.Failure(["Procurement with this reference number not found."]);
        }

        var procurementDto = _mapper.Map<ProcurementDto>(procurement);
        return BaseResponse<ProcurementDto>.Success(procurementDto, "Procurement retrieved successfully.");
    }
}