using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Procurements.Queries.GetProcurementsByDateRange;

public class GetProcurementsByDateRangeHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetProcurementsByDateRangeQuery, List<ProcurementDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<List<ProcurementDto>>> Handle(GetProcurementsByDateRangeQuery request, CancellationToken cancellationToken)
    {
        var procurements = await _unitOfWork.Procurements.GetProcurementsByDateRangeAsync(request.StartDate, request.EndDate);
        if (procurements is null || !procurements.Any())
        {
            return BaseResponse<List<ProcurementDto>>.Failure(["No procurements found for the specified date range."]);
        }

        var procurementDtos = _mapper.Map<List<ProcurementDto>>(procurements);
        return BaseResponse<List<ProcurementDto>>.Success(procurementDtos, $"Found {procurementDtos.Count} procurements in the specified date range.");
    }
}