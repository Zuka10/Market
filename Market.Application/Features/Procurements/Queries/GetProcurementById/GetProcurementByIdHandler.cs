using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Procurements.Queries.GetProcurementById;

public class GetProcurementByIdHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetProcurementByIdQuery, ProcurementDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<ProcurementDto>> Handle(GetProcurementByIdQuery request, CancellationToken cancellationToken)
    {
        var procurement = await _unitOfWork.Procurements.GetProcurementWithDetailsAsync(request.Id);
        if (procurement is null)
        {
            return BaseResponse<ProcurementDto>.Failure(["Procurement not found."]);
        }

        var procurementDto = _mapper.Map<ProcurementDto>(procurement);
        return BaseResponse<ProcurementDto>.Success(procurementDto, "Procurement retrieved successfully.");
    }
}