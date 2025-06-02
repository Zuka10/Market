using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;

namespace Market.Application.Features.ProcurementDetails.Queries.GetProcurementDetailsByProduct;

public class GetProcurementDetailsByProductHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetProcurementDetailsByProductQuery, List<ProcurementDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<List<ProcurementDetailDto>>> Handle(GetProcurementDetailsByProductQuery request, CancellationToken cancellationToken)
    {
        // First verify the product exists
        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId);
        if (product is null)
        {
            return BaseResponse<List<ProcurementDetailDto>>.Failure(["Product not found."]);
        }

        var procurementDetails = await _unitOfWork.ProcurementDetails.GetByProductAsync(request.ProductId);
        var procurementDetailDtos = _mapper.Map<List<ProcurementDetailDto>>(procurementDetails);

        return BaseResponse<List<ProcurementDetailDto>>.Success(procurementDetailDtos, $"Found {procurementDetailDtos.Count} procurement details for product.");
    }
}