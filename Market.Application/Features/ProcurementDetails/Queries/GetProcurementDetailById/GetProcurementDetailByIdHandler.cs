using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;

namespace Market.Application.Features.ProcurementDetails.Queries.GetProcurementDetailById;

public class GetProcurementDetailByIdHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetProcurementDetailByIdQuery, ProcurementDetailDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<ProcurementDetailDto>> Handle(GetProcurementDetailByIdQuery request, CancellationToken cancellationToken)
    {
        var procurementDetail = await _unitOfWork.ProcurementDetails.GetByIdAsync(request.Id);
        if (procurementDetail is null)
        {
            return BaseResponse<ProcurementDetailDto>.Failure(["Procurement detail not found."]);
        }

        var procurementDetailDto = _mapper.Map<ProcurementDetailDto>(procurementDetail);
        return BaseResponse<ProcurementDetailDto>.Success(procurementDetailDto, "Procurement detail retrieved successfully.");
    }
}