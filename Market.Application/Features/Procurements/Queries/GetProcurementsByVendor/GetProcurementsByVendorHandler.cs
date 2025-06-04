using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;
using Market.Domain.Filters;

namespace Market.Application.Features.Procurements.Queries.GetProcurementsByVendor;

public class GetProcurementsByVendorHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetProcurementsByVendorQuery, List<ProcurementDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<List<ProcurementDto>>> Handle(GetProcurementsByVendorQuery request, CancellationToken cancellationToken)
    {
        var procurements = await _unitOfWork.Procurements.GetProcurementsByVendorAsync(request.VendorId);
        var procurementDtos = _mapper.Map<List<ProcurementDto>>(procurements);

        return BaseResponse<List<ProcurementDto>>.Success(procurementDtos, $"Found {procurementDtos.Count} procurements for vendor.");
    }
}