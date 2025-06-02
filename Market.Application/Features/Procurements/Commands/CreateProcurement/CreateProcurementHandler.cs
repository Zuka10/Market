using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;
using Market.Domain.Entities.Market;

namespace Market.Application.Features.Procurements.Commands.CreateProcurement;

public class CreateProcurementHandler(IUnitOfWork unitOfWork, IMapper mapper) : ICommandHandler<CreateProcurementCommand, ProcurementDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<ProcurementDto>> Handle(CreateProcurementCommand request, CancellationToken cancellationToken)
    {
        var procurement = new Procurement
        {
            VendorId = request.VendorId,
            LocationId = request.LocationId,
            ReferenceNo = request.ReferenceNo.Trim().ToUpperInvariant(),
            ProcurementDate = request.ProcurementDate,
            TotalAmount = request.TotalAmount,
            Notes = request.Notes?.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdProcurement = await _unitOfWork.Procurements.AddAsync(procurement);

        // Add procurement details
        foreach (var detail in request.ProcurementDetails)
        {
            var procurementDetail = new ProcurementDetail
            {
                ProcurementId = createdProcurement.Id,
                ProductId = detail.ProductId,
                PurchasePrice = detail.PurchasePrice,
                Quantity = detail.Quantity,
                LineTotal = detail.LineTotal
            };
            await _unitOfWork.ProcurementDetails.AddAsync(procurementDetail);
        }

        var procurementDto = _mapper.Map<ProcurementDto>(createdProcurement);
        return BaseResponse<ProcurementDto>.Success(procurementDto, "Procurement created successfully.");
    }
}